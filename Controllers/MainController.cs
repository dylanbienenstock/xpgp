using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using xpgp.Models;

namespace xpgp
{
    public class MainController : Controller
    {
        DatabaseContext _context;

        public MainController(DatabaseContext context)
        {
            _context = context;

            UserManager.SetDatabaseContext(_context);
        }

        // public string FormatTimeSpan(TimeSpan span)
        // {
        //     if (span == TimeSpan.Zero) return "0 minutes";

        //     var sb = new StringBuilder();

        //     if (span.Days > 0)
        //         sb.AppendFormat("{0} day{1}, ", span.Days, span.Days > 1 ? "s" : String.Empty);
        //     if (span.Hours > 0)
        //         sb.AppendFormat("{0} hour{1}, ", span.Hours, span.Hours > 1 ? "s" : String.Empty);
        //     if (span.Minutes > 0)
        //         sb.AppendFormat("{0} minute{1}, ", span.Minutes, span.Minutes > 1 ? "s" : String.Empty);

        //     string formatted = sb.ToString();

        //     return formatted.TrimEnd(new char[] { ',', ' ' });
        // }

        public string FormatTimeSpan(TimeSpan span)
        {
            if (span.TotalMinutes <= 1) return "< 1 minute";
            
            int number = (int)Math.Round(span.TotalMinutes);
            string units = "minute";

            if (span.Days > 0)
            {
                if (span.TotalDays > 30)
                {
                    number = (int)Math.Round(span.TotalDays / 30.0f);
                    units = "month";

                    if (number >= 12)
                    {
                        number = (int)Math.Round(number / 12.0f);
                        units = "year";
                    }
                }
                else
                {
                    number = (int)Math.Round(span.TotalDays);
                    units = "day";
                }
            }
            else if (span.Hours > 0)
            {
                number = (int)Math.Round(span.TotalHours);
                units = "hour";
            }

            return number.ToString() + " " + units + (number > 1 ? "s" : "");
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

        public void RemoveExpiredKeyPairs(Identity identity)
        {
            _context.KeyPairs.RemoveRange(_context.KeyPairs
            .Where(kp => kp.UserId == identity.UserId && kp.Expiration <= DateTime.Now));

            _context.SaveChanges();
        }

        [HttpGet]
        [Route("")]
        public IActionResult Index()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            RemoveExpiredKeyPairs(identity);

            List<KeyPair> keyPairs = _context.KeyPairs
            .Where(kp => kp.UserId == identity.UserId).ToList();

            if (keyPairs.Count() == 0)
            {
                return RedirectToAction("NewKeyPair");
            }

            Dictionary<int, string> expirationTimes = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => FormatTimeSpan(kp.Expiration - DateTime.Now)
            );

            Dictionary<int, string> viewUrls = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => this.Url.Action("ViewKey", "Main", new
                {
                    UserId = identity.UserId,
                    KeyPairId = kp.KeyPairId
                })
            );

            Dictionary<int, string> downloadUrls = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => this.Url.Action("DownloadKey", "Main", new
                {
                    UserId = identity.UserId,
                    KeyPairId = kp.KeyPairId
                })
            );

            ViewBag.KeyPairs = keyPairs;
            ViewBag.ExpirationTimes = expirationTimes;
            ViewBag.ViewUrls = viewUrls;
            ViewBag.DownloadUrls = downloadUrls;
            
            return View();
        }

        [HttpGet]
        [Route("NewKeyPair")]
        public IActionResult NewKeyPair()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpGet]
        [Route("ViewKey/{UserId}/{KeyPairId}")]
        public IActionResult ViewKey(int UserId, int KeyPairId)
        {
            // Identity identity = UserManager.Validate(HttpContext.Session);

            // if (!identity.Valid)
            // {
            //     return RedirectToAction("Login", "Account");
            // }

            KeyPair keyPair = FindKeyPair(UserId, KeyPairId);

            if (keyPair != null)
            {
                string publicKey = (new AES()).DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt);

                return Content(publicKey);
            }

            return Content("Not found.");
        }

        [HttpGet]
        [Route("DownloadKey/{UserId}/{KeyPairId}")]
        public IActionResult DownloadKey(int UserId, int KeyPairId)
        {
            try
            {
                AES aes = new AES();

                KeyPair keyPair = _context.KeyPairs.Single(kp => kp.UserId == UserId && 
                                                           kp.KeyPairId == KeyPairId);

                string publicKey = aes.DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt);
                byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
                string fileName = keyPair.Name + "_public.asc";

                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    fileName = fileName.Replace(c.ToString(), string.Empty);
                }

                fileName = fileName.Replace(' ', '_');
                fileName = fileName.Replace("\'", string.Empty);
                fileName = fileName.Replace("\"", string.Empty);

                return File(publicKeyBytes, MediaTypeNames.Text.Plain, fileName);
            }
            catch
            {
                // Oops.
            }

            return Content("Could not download file.");
        }

        // Jesus christ, what a nightmare. Let's save that for another time.

        // https://stackoverflow.com/questions/17232414/creating-a-zip-archive-in-memory-using-system-io-compression
        // https://stackoverflow.com/questions/620605/how-to-make-a-valid-windows-filename-from-an-arbitrary-string
        // https://stackoverflow.com/questions/2456842/asp-net-create-zip-file-for-download-the-compressed-zipped-folder-is-invalid-or
        // https://stackoverflow.com/questions/10934585/memorystream-cannot-access-a-closed-stream
        // https://stackoverflow.com/questions/27098306/c-sharp-create-zip-archive-with-multiple-files
        // [HttpGet]
        // [Route("DownloadKeyPair/{UserId}/{KeyPairId}")]
        // public IActionResult DownloadKeyPair(int UserId, int KeyPairId)
        // {
        //     // try
        //     // {
        //         AES aes = new AES();

        //         KeyPair keyPair = _context.KeyPairs.Single(kp => kp.UserId == UserId && 
        //                                                    kp.KeyPairId == KeyPairId);

        //         string publicKey = aes.DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt);
        //         string privateKey = aes.DecryptString(keyPair.PrivateKey, AES.Password, keyPair.Salt);
        //         byte[] publicKeyBytes = Encoding.UTF8.GetBytes(publicKey);
        //         byte[] privateKeyBytes = Encoding.UTF8.GetBytes(privateKey);
        //         string fileName = keyPair.Name;

        //         foreach (char c in System.IO.Path.GetInvalidFileNameChars())
        //         {
        //             fileName = fileName.Replace(c, '_');
        //         }

        //         fileName = fileName.Replace(' ', '_');
        //         fileName = fileName.Replace('\'', '_');
        //         fileName = fileName.Replace('\"', '_');

        //         var memoryStream = new MemoryStream();
        //         // using (var memoryStream = new MemoryStream())
        //         // {
        //             using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, false))
        //             {
        //                 var publicKeyEntry = archive.CreateEntry(fileName + "_public.asc");
        //                 using (var entryStream = publicKeyEntry.Open()) entryStream.Write(publicKeyBytes, 0, publicKeyBytes.Length);

        //                 var privateKeyEntry = archive.CreateEntry(fileName + "_private.asc");
        //                 using (var entryStream = privateKeyEntry.Open()) entryStream.Write(privateKeyBytes, 0, privateKeyBytes.Length); 
        //             }

        //             byte[] shit = memoryStream.ToArray();
        //             return File(shit, MediaTypeNames.Application.Zip, fileName + ".zip");
        //         // }
        //     // }
        //     // catch
        //     // {
        //     //     // Oops.
        //     // }

        //     // return Content("Could not download file.");
        // }
    }
}