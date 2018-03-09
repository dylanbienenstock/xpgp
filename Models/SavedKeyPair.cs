using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xpgp.Models
{
    public class SavedKeyPair : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SavedKeyPairId { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        public KeyPair KeyPair { get; set; }

        public int KeyPairId { get; set; }

        public Notification Notification { get; set; }

        public int NotificationId { get; set; }
    }
}