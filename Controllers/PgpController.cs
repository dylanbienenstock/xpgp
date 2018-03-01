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

            _context.Add(keyPair);
            _context.SaveChanges();

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

                return Json(new
                {
                    Success = true,
                    Url = "http://localhost:5000/ViewKeyPair/" + identity.UserId + "/" + keyPair.KeyPairId + "/"
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
                Success = false,
                Errors = errors
            });
        }

        [HttpGet]
        [Route("PGP/EncryptString/{text}")]
        public IActionResult EncryptString(string text)
        {
            AES aes = new AES();

            ViewBag.Text = aes.EncryptString(text, AES.Password, AES.Salt);

            return View("Text");
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