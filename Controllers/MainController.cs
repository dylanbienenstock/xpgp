using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xpgp.Models;
using xpgp.ViewModels;
using SixLabors.ImageSharp;

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

        public void BagUpKeyPairs(List<KeyPair> keyPairs, dynamic ViewBag) {
            Dictionary<int, string> expirationTimes = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => FormatTimeSpan(kp.Expiration - DateTime.Now)
            );

            Dictionary<int, string> deleteUrls = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => this.Url.Action("DeleteKey", "Main", new
                {
                    UserId = kp.UserId,
                    KeyPairId = kp.KeyPairId
                })
            );

            Dictionary<int, string> viewUrls = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => this.Url.Action("ViewKey", "Main", new
                {
                    UserId = kp.UserId,
                    KeyPairId = kp.KeyPairId
                })
            );


            Dictionary<int, string> rawUrls = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => this.Url.Action("ViewKeyRaw", "Main", new
                {
                    UserId = kp.UserId,
                    KeyPairId = kp.KeyPairId
                })
            );

            Dictionary<int, string> downloadUrls = keyPairs.ToDictionary(
                kp => kp.KeyPairId,
                kp => this.Url.Action("DownloadKey", "Main", new
                {
                    UserId = kp.UserId,
                    KeyPairId = kp.KeyPairId
                })
            );

            ViewBag.KeyPairs = keyPairs;
            ViewBag.ExpirationTimes = expirationTimes;
            ViewBag.DeleteUrls = deleteUrls;
            ViewBag.ViewUrls = viewUrls;
            ViewBag.RawUrls = rawUrls;
            ViewBag.DownloadUrls = downloadUrls;
        }

        public List<KeyPair> GetUsableKeyPairs(Identity identity)
        {
            User user = _context.Users.SingleOrDefault(u => u.UserId == identity.UserId);

            List<KeyPair> keyPairs = _context.KeyPairs
                .Where(kp => kp.UserId == identity.UserId)
                .Include(kp => kp.User)
                .ToList();

            List<SavedKeyPair> savedKeyPairs = _context.SavedKeyPairs
                .Where(skp => skp.UserId == identity.UserId)
                .Include(skp => skp.KeyPair)
                .ThenInclude(kp => kp.User)
                .ToList();

            foreach (SavedKeyPair savedKeyPair in savedKeyPairs)
            {
                KeyPair keyPair = _context.KeyPairs
                    .SingleOrDefault(kp => kp.KeyPairId == savedKeyPair.KeyPairId);

                if (keyPair != null)
                {
                    keyPair.SavedAt = savedKeyPair.CreatedAt;
                    keyPair.IsSaved = true;

                    keyPairs.Add(keyPair);
                }
            }

            keyPairs.Sort((a, b) =>
            {
                if (user.PinnedKeyPair != null && a.KeyPairId == user.PinnedKeyPair.KeyPairId) return 1;

                if ((a.SavedAt ?? a.CreatedAt) > (b.SavedAt ?? b.CreatedAt)) return 1;

                return -1;
            });

            keyPairs.Reverse();

            return keyPairs;
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

            UserManager.BagUp(identity, ViewBag);
            RemoveExpiredKeyPairs(identity);

            List<KeyPair> keyPairs = GetUsableKeyPairs(identity);

            if (keyPairs.Count() == 0)
            {
                return RedirectToAction("NewKey");
            }

            BagUpKeyPairs(keyPairs, ViewBag);
            
            return View();
        }

        [HttpGet]
        [Route("Profile/{UserId}")]
        public IActionResult Profile(int UserId)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            User profileOwner = _context.Users.SingleOrDefault(u => u.UserId == UserId);
            ViewBag.ProfileOwner = profileOwner;

            if (profileOwner == null) return Content("User not found.");

            List<SavedKeyPair> savedKeyPairs = _context.SavedKeyPairs
                .Where(skp => skp.UserId == identity.UserId)
                .ToList();

            List<KeyPair> keyPairs = _context.KeyPairs
                .Where(kp => kp.UserId == UserId)
                .ToList();

            foreach (KeyPair keyPair in keyPairs)
            {
                keyPair.IsSaved = savedKeyPairs
                    .Select(skp => skp.KeyPairId)
                    .Contains(keyPair.KeyPairId);
            }
            
            if (keyPairs.Count() == 0)
            {
                ViewBag.ProfileOwnerHasNoKeys = true;
            }
            else
            {
                BagUpKeyPairs(keyPairs, ViewBag);
                ViewBag.PinnedKeyPair = keyPairs.First();
            }

            return View();
        }

        [HttpGet]
        [Route("Profile/Settings")]
        public IActionResult ProfileSettings()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            User user = _context.Users.SingleOrDefault(u => u.UserId == identity.UserId);

            SettingsViewModel model = new SettingsViewModel
            {
                Bio = user.Bio,
                PublicEmailAddress = user.PublicEmailAddress
            };

            return View(model);
        }

        [HttpPost]
        [Route("Profile/Settings/Submit")]
        [ValidateAntiForgeryToken]
        public IActionResult ProfileSettingsSubmit(SettingsViewModel model)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            if (ModelState.IsValid)
            {
                User user = _context.Users.SingleOrDefault(u => u.UserId == identity.UserId);

                if (!string.IsNullOrWhiteSpace(model.Bio))
                {
                    user.Bio = model.Bio.Substring(0, Math.Min(model.Bio.Length, 196));
                    _context.Entry(user).Property(u => u.Bio).IsModified = true;
                }

                if (!string.IsNullOrWhiteSpace(model.PublicEmailAddress))
                {
                    user.PublicEmailAddress = model.PublicEmailAddress;
                    _context.Entry(user).Property(u => u.PublicEmailAddress).IsModified = true;
                }

                if (model.ProfilePicture != null &&
                   (model.ProfilePicture.ContentType == "image/png" ||
                    model.ProfilePicture.ContentType == "image/jpeg"))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        model.ProfilePicture.CopyTo(memoryStream);

                        Image<Rgba32> image = Image.Load(memoryStream.ToArray());
                        image.Mutate(x => x
                            .Resize(128, 128)
                        );

                        using (var memoryStream2 = new MemoryStream())
                        {
                            image.SaveAsPng(memoryStream2);
                            user.ProfilePicture = memoryStream2.ToArray();
                        }
                    }

                    _context.Entry(user).Property(u => u.ProfilePicture).IsModified = true;
                }

                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpGet]
        [Route("ProfileImage/{UserId}")]
        public IActionResult ProfileImage(int UserId)
        {
            User user = _context.Users.SingleOrDefault(u => u.UserId == UserId);

            if (user != null && user.ProfilePicture != null)
            {
                return File(user.ProfilePicture, "image/png");
            }

            var file = Path.Combine(Directory.GetCurrentDirectory(),
                           "wwwroot", "img", "profile.svg");

            return PhysicalFile(file, "image/svg+xml");
        }

        [HttpGet]
        [Route("Search")]
        public IActionResult Search(string query)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            var results = _context.Users.Where(u => 
                EF.Functions.Like((u.FirstName + " " + u.LastName).ToLower(), "%" + query.ToLower() + "%")
            ).Include(u => u.PinnedKeyPair);

            List<SavedKeyPair> savedKeyPairs = _context.SavedKeyPairs
                .Where(skp => skp.UserId == identity.UserId)
                .ToList();

            foreach (KeyPair keyPair in results.Select(u => u.PinnedKeyPair))
            {
                if (keyPair != null)
                {
                    keyPair.IsSaved = savedKeyPairs
                        .Select(skp => skp.KeyPairId)
                        .Contains(keyPair.KeyPairId);                    
                }
            }

            BagUpKeyPairs(results
                .Select(u => u.PinnedKeyPair)
                .Where(kp => kp != null)
                .ToList(), ViewBag);

            ViewBag.HasResults = results.Count() > 0;
            ViewBag.SearchResults = results;
            ViewBag.Query = query;

            return View();
        }

        [HttpPost]
        [Route("PinKeyPair")]
        public IActionResult PinKeyPair(int KeyPairId)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            KeyPair keyPair = _context.KeyPairs.SingleOrDefault(kp => kp.KeyPairId == KeyPairId);

            if (keyPair != null && keyPair.UserId == identity.UserId) // Ensure that they own it
            {
                User user = _context.Users.SingleOrDefault(u => u.UserId == identity.UserId);

                user.PinnedKeyPair = keyPair;
                _context.Entry(user).Reference(u => u.PinnedKeyPair).IsModified = true;
                _context.SaveChanges();
            }

            // return RedirectToAction("Profile", new { UserId = identity.UserId });
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpPost]
        [Route("UnPinKeyPair")]
        public IActionResult UnPinKeyPair(int KeyPairId)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            KeyPair keyPair = _context.KeyPairs.SingleOrDefault(kp => kp.KeyPairId == KeyPairId);

            if (keyPair != null && keyPair.UserId == identity.UserId) // Ensure that they own it
            {
                User user = _context.Users.SingleOrDefault(u => u.UserId == identity.UserId);

                user.PinnedKeyPair = null;
                _context.Entry(user).Reference(u => u.PinnedKeyPair).IsModified = true;
                _context.SaveChanges();
            }

            // return RedirectToAction("Profile", new { UserId = identity.UserId });
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet]
        [Route("NewKey")]
        public IActionResult NewKey()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            return View();
        }

        [HttpGet]
        [Route("Encrypt")]
        public IActionResult Encrypt()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            List<KeyPair> keyPairs = GetUsableKeyPairs(identity);

            if (keyPairs.Count() == 0)
            {
                return RedirectToAction("NewKey");
            }

            BagUpKeyPairs(keyPairs, ViewBag);

            ViewBag.EncryptUrl = this.Url.Action("EncryptString", "PGP");

            return View();
        }

        [HttpGet]
        [Route("Decrypt")]
        public IActionResult Decrypt()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (!identity.Valid)
            {
                return RedirectToAction("Login", "Account");
            }

            UserManager.BagUp(identity, ViewBag);

            List<KeyPair> keyPairs = _context.KeyPairs
                .Where(kp => kp.UserId == identity.UserId).ToList();

            if (keyPairs.Count() == 0)
            {
                return RedirectToAction("NewKey");
            }

            BagUpKeyPairs(keyPairs, ViewBag);

            ViewBag.DecryptUrl = this.Url.Action("DecryptString", "PGP");

            return View();
        }

        [HttpPost]
        [Route("SaveKeyPair")]
        public IActionResult SaveKeyPair(int UserId, int KeyPairId)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (identity.Valid && UserId != identity.UserId) // Make sure the user DOESN'T own it
            {
                SavedKeyPair existingSavedKeyPair = _context.SavedKeyPairs
                    .SingleOrDefault(skp => 
                        skp.UserId == identity.UserId &&
                        skp.KeyPairId == KeyPairId
                    );

                if (existingSavedKeyPair == null)
                {
                    KeyPair keyPair = FindKeyPair(UserId, KeyPairId);

                    if (keyPair != null)
                    {
                        Notification notification = new Notification
                        {
                            UserId = keyPair.UserId,
                            AssociatedUserId = identity.UserId,
                            NotificationType = NotificationType.KeySaved,
                            Text = "@ saved your key: " + keyPair.Name
                        };

                        SavedKeyPair savedKeyPair = new SavedKeyPair
                        {
                            UserId = identity.UserId,
                            KeyPairId = KeyPairId,
                            Notification = notification
                        };

                        _context.Notifications.Add(notification);
                        _context.SavedKeyPairs.Add(savedKeyPair);
                        _context.SaveChanges();

                        return Content("KeyPair saved.");
                    }
                }

                return Content("KeyPair already saved.");
            }

            return Content("No permission.");
        }

        [HttpPost]
        [Route("UnSaveKeyPair")]
        public IActionResult UnSaveKeyPair(int UserId, int KeyPairId)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (identity.Valid)
            {
                SavedKeyPair existingSavedKeyPair = _context.SavedKeyPairs
                    .SingleOrDefault(skp => 
                        skp.UserId == identity.UserId &&
                        skp.KeyPairId == KeyPairId
                    );

                if (existingSavedKeyPair != null)
                {
                    _context.Notifications.Remove(existingSavedKeyPair.Notification);
                    _context.SavedKeyPairs.Remove(existingSavedKeyPair);
                    _context.SaveChanges();
                }

                return Content("KeyPair unsaved.");
            }

            return Content("No permission.");
        }

        [HttpGet]
        [Route("ViewKey/Raw")]
        public IActionResult ViewKeyRaw(int UserId, int KeyPairId)
        {
            KeyPair keyPair = FindKeyPair(UserId, KeyPairId);

            if (keyPair != null)
            {
                string publicKey = (new AES()).DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt);

                return Content(publicKey);
            }

            return Content("Not found.");
        }

        [HttpGet]
        [Route("ViewKey")]
        public IActionResult ViewKey(int UserId, int KeyPairId)
        {
            KeyPair keyPair = FindKeyPair(UserId, KeyPairId);

            if (keyPair != null)
            {
                ViewBag.LoggedIn = UserManager.Validate(HttpContext.Session).Valid;
                ViewBag.PublicKey = (new AES()).DecryptString(keyPair.PublicKey, AES.Password, keyPair.Salt);

                return View(keyPair);
            }

            return Content("Not found.");
        }

        [HttpGet]
        [Route("DeleteKey/{UserId}/{KeyPairId}")]
        public IActionResult DeleteKey(int UserId, int KeyPairId)
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (identity.Valid && UserId == identity.UserId) // Make sure the user owns it
            {
                KeyPair keyPair = FindKeyPair(UserId, KeyPairId);

                if (keyPair != null) {
                    _context.KeyPairs.Remove(keyPair);
                    _context.SaveChanges();

                    // return RedirectToAction("Index", "Main");
                    return Redirect(Request.Headers["Referer"].ToString());
                }

                return Content("Not found.");
            }

            return Content("No permission.");
        }

        [HttpGet]
        [Route("DownloadKey/{UserId}/{KeyPairId}")]
        public IActionResult DownloadKey(int UserId, int KeyPairId)
        {
            try
            {
                AES aes = new AES();

                // Throw exception if not found
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
    }
}