using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using xpgp.Models;
using xpgp.ViewModels;

namespace xpgp
{
	public class AccountController : Controller
    {
        private DatabaseContext _context;

        public AccountController(DatabaseContext context)
        {
            _context = context;

            UserManager.SetDatabaseContext(_context);
        }

        public bool UniqueEmailAddress(RegisterViewModel model)
        {
            return _context.Users.SingleOrDefault(u => u.EmailAddress == model.EmailAddress) == null;
        }

        [HttpGet]
        [Route("Account/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/Register/Submit")]
        public IActionResult RegisterSubmit(RegisterViewModel model)
        {
            if (!UniqueEmailAddress(model))
            {
                ModelState.AddModelError("EmailAddress", "Email address already in use.");
            }

            if (ModelState.IsValid)
            {
                // Successful registration
                UserManager.CreateAccount(model);
                UserManager.Login(model.EmailAddress, model.Password, HttpContext.Session);

                return RedirectToAction("Index", "Main");
            }

            ViewBag.HasErrors = true;

            // Error
            return View("Register", model);
        }

        [HttpGet]
        [Route("Account/Login")]
        public IActionResult Login()
        {
            Identity identity = UserManager.Validate(HttpContext.Session);

            if (identity.Valid) // Already logged in
            {
                return RedirectToAction("AccessDenied");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Account/Login/Submit")]
        public IActionResult LoginSubmit(LoginViewModel model)
        {
            if (UserManager.Login(model.EmailAddress, model.Password, HttpContext.Session))
            {
                // Successful login
                return RedirectToAction("Index", "Main");
            }

            ViewBag.LoginError = "Incorrect login details.";

            // Error
            return View("Login", model);
        }

        [HttpGet]
        [Route("Account/LogOut")]
        public IActionResult LogOut()
        {
            UserManager.LogOut(HttpContext.Session);

            return RedirectToAction("Index", "Main");
        }

        [HttpGet]
        [Route("Account/AccessDenied")]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Main");
        }
    }
}