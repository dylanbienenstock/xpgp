using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace xpgp.Models
{
    public enum NotificationType
    {
        Account, // Various account-related notifications
        AccountSetup, // Account completion notifications
        KeySaved, // Someone saved your key
        KeyExpired, // Your key or a saved key expired
        EmailRequested, // Someone wants your email address
        QAndAVerification, // Someone wants to verify it's you with a secret question
    }

    public static class NotificationHelpers {
        private static DatabaseContext _context = null;
        private static IUrlHelper _urlHelper = null;

        public static void SetDatabaseContext(DatabaseContext context)
        {
            _context = context;
        }

        public static void SetUrlHelper(IUrlHelper urlHelper)
        {
            _urlHelper = urlHelper;
        }

        public static Dictionary<NotificationType, string> Formats =
        new Dictionary<NotificationType, string>
        {
            { NotificationType.Account, "" },
            { NotificationType.AccountSetup, "Additional action is required to complete your account. %Settings" },
            { NotificationType.KeySaved, "%User has saved your key: %Key" },
            { NotificationType.KeyExpired, "%User's key has expired: %Key" },
            { NotificationType.EmailRequested, "" },
            { NotificationType.QAndAVerification, "" },
        };

        public static IHtmlContent NotificationContent(this IHtmlHelper htmlHelper, Notification notification)
        {
            string html = Formats[notification.NotificationType];

            if (html.Contains("%Settings"))
            {
                string text = "&rarr;&nbsp;Settings";
                string url = $"/Profile/Settings";
                string href = '\"' + url + '\"';

                html = html.Replace("%Settings", $@"<a href={href}>{text}</a>");
            }

            if (html.Contains("%User"))
            {
                string name = $"{notification.AssociatedUser.FirstName} {notification.AssociatedUser.LastName}";
                string url = $"/Profile/{notification.AssociatedUser.UserId}"; // TODO: Do this dynamically
                string href = '\"' + url + '\"';

                html = html.Replace("%User", $@"<a href={href}>{name}</a>");
            }

            if (html.Contains("%Key"))
            {
                if (notification.NotificationType != NotificationType.KeyExpired)
                {
                    try
                    {
                        KeyPair keyPair = _context.KeyPairs.Single(
                            kp => kp.KeyPairId == notification.AssociatedModelId
                        );

                        string url = $"/Profile/{keyPair.UserId}?SelectedKeyPairId={keyPair.KeyPairId}";
                        string href = '\"' + url + '\"';

                        html = html.Replace("%Key", $@"<a href={href}>{keyPair.Name}</a>");
                    }
                    catch // Remove notification if key is expired
                    {
                        _context.Notifications.Remove(notification);
                        _context.SaveChanges();
                    }
                }
                else // Key doesn't exist in database, so find its name in ExpiredKeyNames table
                {

                }
            }
            
            return new HtmlString(html);
        }
    }

    public class Notification : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        public User AssociatedUser { get; set; }
        
        public int? AssociatedUserId { get; set; }

        public int? AssociatedModelId { get; set; }

        public NotificationType NotificationType { get; set; }

        public bool Seen { get; set; }
    }
}