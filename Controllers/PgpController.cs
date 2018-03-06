using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PgpCore;

using xpgp.Models;
using xpgp.ViewModels;

namespace xpgp
{
    public class PgpController : Controller
    {
        private static Random random = new Random();
        private DatabaseContext _context;

        public PgpController(DatabaseContext context)
        {
            _context = context;

            UserManager.SetDatabaseContext(_context);
        }
        
        public KeyPair GenerateKeyPair(int userId, string name, string emailAddress, string password, int expiration, string expirationUnits)
        {
            AES aes = new AES();
            string publicKey;
            string privateKey;

            using (PGP pgp = new PGP())
            using (MemoryStream publicKeyStream = new MemoryStream())
            using (MemoryStream privateKeyStream = new MemoryStream())
            {
                pgp.GenerateKey(publicKeyStream, privateKeyStream, emailAddress, password);

                publicKey = Encoding.UTF8.GetString(publicKeyStream.ToArray());
                privateKey = Encoding.UTF8.GetString(privateKeyStream.ToArray());
            }

            byte[] saltBytes = new byte[16];
            random.NextBytes(saltBytes); // TODO: Figure out a more crypto way to do this
            string salt = Encoding.UTF8.GetString(saltBytes);

            TimeSpan expirationTimeSpan;

            switch (expirationUnits)
            {
                case "minutes":
                    expirationTimeSpan = TimeSpan.FromMinutes(expiration);
                    break;
                case "hours":
                    expirationTimeSpan = TimeSpan.FromHours(expiration);
                    break;
                case "days":
                    expirationTimeSpan = TimeSpan.FromDays(expiration);
                    break;
                case "months":
                    expirationTimeSpan = TimeSpan.FromDays(expiration * 30);
                    break;
                default: // "years"
                    expirationTimeSpan = TimeSpan.FromDays(expiration * 365);
                    break;
            }

            User user = _context.Users.Single(u => u.UserId == userId);

            KeyPair keyPair =  new KeyPair
            {
                User = user,
                Name = name,
                EmailAddress = aes.EncryptString(emailAddress, AES.Password, salt),
                Password = aes.EncryptString(password, AES.Password, salt),
                PublicKey = aes.EncryptString(publicKey, AES.Password, salt),
                PrivateKey = aes.EncryptString(privateKey, AES.Password, salt),
                Salt = salt,
                Expiration = DateTime.Now.Add(expirationTimeSpan),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                _context.Add(keyPair);
                _context.SaveChanges();
            } 
            catch
            {
                // TODO: MAKE THIS LESS DANGEROUS 
                GenerateKeyPair(userId, name, emailAddress, password, expiration, expirationUnits);
            }

            return keyPair;
        }

        [HttpPost]
        [Route("PGP/NewKeyPair")]
        public IActionResult NewKeyPair(KeyPairViewModel model)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                KeyPair keyPair = GenerateKeyPair(
                    identity.UserId,
                    model.Name,
                    model.EmailAddress,
                    model.Password,
                    model.Expiration,
                    model.ExpirationUnits
                );

                User user = _context.Users.SingleOrDefault(u => u.UserId == identity.UserId);

                if (user.PinnedKeyPair == null)
                {
                    user.PinnedKeyPair = keyPair;

                    _context.Entry(user).Reference(u => u.PinnedKeyPair).IsModified = true;
                    _context.SaveChanges();
                }

                return Json(new
                {
                    success = true,
                    publicKey = (new AES()).DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt),
                    viewUrl = this.Url.Action("ViewKey", "Main", new {
                        UserId = identity.UserId,
                        KeyPairId = keyPair.KeyPairId 
                    }),
                    downloadUrl = this.Url.Action("DownloadKey", "Main", new {
                        UserId = identity.UserId,
                        KeyPairId = keyPair.KeyPairId
                    })
                });
            }

            List<string> errors = new List<string>();

            foreach (var modelState in ViewData.ModelState.Values)
            {
                foreach (ModelError error in modelState.Errors)
                {
                    errors.Add(error.ErrorMessage);
                }
            }

            return Json(new
            {
                success = false,
                errors = errors
            });
        }

        public KeyPair FindKeyPair(int UserId, int KeyPairId)
        {
            User user = _context.Users.SingleOrDefault(u => u.UserId == UserId);

            if (user != null)
            {
                return _context.KeyPairs.SingleOrDefault(kp => kp.KeyPairId == KeyPairId
                                                        && kp.UserId == UserId);
            }

            return null;
        }

        // https://stackoverflow.com/questions/1879395/how-do-i-generate-a-stream-from-a-string
        public static Stream StringToStream(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        [HttpPost]
        [Route("PGP/EncryptString/")]
        public IActionResult EncryptString(int userId, int keyPairId, string text)
        {
            KeyPair keyPair = FindKeyPair(userId, keyPairId);

            if (keyPair == null) return Content("Key not found");

            string publicKey = (new AES()).DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt);
            string encrypted = null;

            using (var textStream = StringToStream(text))
            using (var publicKeyStream = StringToStream(publicKey))
            using (var outputStream = new MemoryStream())
            {
                (new PGP()).EncryptStream(textStream, outputStream, publicKeyStream);
                encrypted = Encoding.UTF8.GetString(outputStream.ToArray());
            }

            return Content(encrypted);
        }

        [HttpPost]
        [Route("PGP/DecryptString/")]
        public IActionResult DecryptString(int userId, int keyPairId, string text)
        {
            try
            {
                KeyPair keyPair = FindKeyPair(userId, keyPairId);
                AES aes = new AES();

                if (keyPair == null) return Content("Key not found");

                string privateKey = aes.DecryptString(keyPair.PrivateKey, AES.Password, keyPair.Salt);
                string password = aes.DecryptString(keyPair.Password, AES.Password, keyPair.Salt);
                string encrypted = null;

                using (var textStream = StringToStream(text))
                using (var privateKeyStream = StringToStream(privateKey))
                using (var outputStream = new MemoryStream())
                {
                    (new PGP()).DecryptStream(textStream, outputStream, privateKeyStream, password);
                    encrypted = Encoding.UTF8.GetString(outputStream.ToArray());
                }

                return Content(encrypted);
            }
            catch
            {

            }

            return Content("----- ERROR -----");
        }

        [HttpGet]
        [Route("PGP/GetEncryptedPrivateKey")]
        public IActionResult GetEncryptedPrivateKey()
        {
            AES aes = new AES();

            string publicKey;
            string privateKey;

            using (PGP pgp = new PGP())
            using (MemoryStream publicKeyStream = new MemoryStream())
            using (MemoryStream privateKeyStream = new MemoryStream())
            {
                pgp.GenerateKey(publicKeyStream, privateKeyStream, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

                publicKey = Encoding.UTF8.GetString(publicKeyStream.ToArray());
                privateKey = Encoding.UTF8.GetString(privateKeyStream.ToArray());
            }

            string encrypted = aes.EncryptString(privateKey, AES.Password, "1234567890asdfgh");

            ViewBag.Text = encrypted;
            ViewBag.Text += "\nLength: " + encrypted.Length + "\n\n";
            ViewBag.Text += publicKey;
            ViewBag.Text += "\nLength: " + publicKey.Length + "\n\n";

            return View("Text");
        }
    }
}