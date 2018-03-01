using System;
using System.Collections.Generic;
using System.Linq;
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

        public string FormatTimeSpan(TimeSpan span)
        {
            if (span == TimeSpan.Zero) return "0 minutes";

            var sb = new StringBuilder();

            if (span.Days > 0)
                sb.AppendFormat("{0} day{1}, ", span.Days, span.Days > 1 ? "s" : String.Empty);
            if (span.Hours > 0)
                sb.AppendFormat("{0} hour{1}, ", span.Hours, span.Hours > 1 ? "s" : String.Empty);
            if (span.Minutes > 0)
                sb.AppendFormat("{0} minute{1}, ", span.Minutes, span.Minutes > 1 ? "s" : String.Empty);
            
            string formatted = sb.ToString();

            return formatted.TrimEnd(new char[] { ',', ' ' });
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
            .Where(kp => kp.UserId == identity.UserId && kp.Expiration < DateTime.Now));

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

            ViewBag.KeyPairs = keyPairs;
            ViewBag.ExpirationTimes = expirationTimes;
            
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
        [Route("ViewKeyPair/{UserId}/{KeyPairId}")]
        public IActionResult ViewKeyPair(int UserId, int KeyPairId)
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
    }
}