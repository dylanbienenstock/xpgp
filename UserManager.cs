using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using xpgp.Models;
using xpgp.ViewModels;

namespace xpgp
{
	public class Identity
    {
        public int UserId;
        public string FirstName;
        public string LastName;
        public string EmailAddress;
        public bool Valid; // True if logged in

        public Identity()
        {
            Valid = false;
        }
    }

    public static class UserManager
    {
        private static DatabaseContext _context;

        public static void SetDatabaseContext(DatabaseContext context)
        { 
            _context = context;
        }

        public static void CreateAccount(RegisterViewModel model)
        {
            User user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailAddress = model.EmailAddress,
                PasswordHash = SecurePasswordHasher.Hash(model.Password),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Add(user);
            _context.SaveChanges();
        }

        public static void CreateLoginToken(User user)
        {
            user.LoginToken = SecurePasswordHasher.Hash(DateTime.UtcNow.ToString() +
                                                        user.UserId.ToString() +
                                                        user.EmailAddress);
            user.UpdatedAt = DateTime.Now;

            _context.Entry(user).Property("LoginToken").IsModified = true;
            _context.Entry(user).Property("UpdatedAt").IsModified = true;
            _context.SaveChanges();
        }

        public static void DestroyLoginToken(ISession session)
        {
            User user;

            try // to clear their login token.
            {
                user = _context.Users.Single(u =>
                    u.EmailAddress == session.GetString("User:EmailAddress") &&
                    u.LoginToken == session.GetString("User:LoginToken")
                );

                user.LoginToken = SecurePasswordHasher.Hash(DateTime.UtcNow.ToString() +
                                            user.UserId.ToString() +
                                            user.EmailAddress);
                user.UpdatedAt = DateTime.Now;

                _context.Entry(user).Property("LoginToken").IsModified = true;
                _context.Entry(user).Property("UpdatedAt").IsModified = true;
                _context.SaveChanges();
            }
            catch
            {
                // Do nothing
            }
        }

        public static bool Login(string emailAddress, string password, ISession session)
        {
            User user;

            try
            {
                user = _context.Users.Single(u =>
                    u.EmailAddress == emailAddress &&
                    SecurePasswordHasher.Verify(password, u.PasswordHash)
                );
            }
            catch
            {
                return false; // Not found
            }

            LogOut(session);
            CreateLoginToken(user);

            session.SetInt32("User:Id", user.UserId);
            session.SetString("User:FirstName", user.FirstName);
            session.SetString("User:LastName", user.LastName);
            session.SetString("User:EmailAddress", user.EmailAddress);
            session.SetString("User:LoginToken", user.LoginToken);

            return true;
        }

        public static void LogOut(ISession session)
        {
            DestroyLoginToken(session);
            session.Clear();
        }

        public static Identity Validate(ISession session)
        {
            // Invalid by default
            Identity identity = new Identity();

            try
            {
                identity.UserId = (int)(session.GetInt32("User:Id") ?? -1);
                identity.FirstName = session.GetString("User:FirstName");
                identity.LastName = session.GetString("User:LastName");
                identity.EmailAddress = session.GetString("User:EmailAddress");
                string token = session.GetString("User:LoginToken");

                _context.Users.Single(u =>
                    u.UserId == identity.UserId &&
                    u.EmailAddress == identity.EmailAddress &&
                    u.LoginToken == session.GetString("User:LoginToken")
                );

                // Throws exception if user does not exist,
                // or if session strings are null.
                // If we made it this far, the identity is valid.

                identity.Valid = true;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }

            return identity;
        }

        // Inserts identity object into ViewBag
        public static void BagUp(Identity identity, dynamic ViewBag)
        {
            ViewBag.UserId = identity.UserId;
            ViewBag.UserFirstName = identity.FirstName;
            ViewBag.UserLastName = identity.LastName;
            ViewBag.UserEmailAddress = identity.EmailAddress;
        }
    }
}