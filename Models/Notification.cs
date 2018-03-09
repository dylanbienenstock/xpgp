using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public class Notification : BaseModel
    {
        private static Dictionary<NotificationType, string> Titles = 
        new Dictionary<NotificationType, string>()
        {
            { NotificationType.Account, "Account" },
            { NotificationType.AccountSetup, "Set up your account" },
            { NotificationType.KeySaved, "Key saved" },
            { NotificationType.KeyExpired, "Key expired" },
            { NotificationType.EmailRequested, "Email address requested" },
            { NotificationType.QAndAVerification, "Verification requested" },
        };

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationId { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        public User AssociatedUser { get; set; }
        
        public int AssociatedUserId { get; set; }

        public NotificationType NotificationType { get; set; }

        [NotMapped]
        public string Title {
            get {
                return Titles[this.NotificationType];
            }
        }

        public string Text { get; set; }
    }
}