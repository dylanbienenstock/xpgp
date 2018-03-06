using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xpgp.Models
{
    public class KeyPair : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KeyPairId { get; set; }

        public string Name { get; set; }

        public string EmailAddress { get; set; }

        public string Password { get; set; }

        public DateTime Expiration { get; set; }

        [MaxLength(1024)]
        public string PublicKey { get; set; }
 
        [MaxLength(2048)]
        public string PrivateKey { get; set; }

        public string Salt { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        [NotMapped]
        public bool IsSaved { get; set; }

        [NotMapped]
        public DateTime? SavedAt { get; set; }

        [NotMapped]
        public bool IsPinned
        { 
            get
            {
                return User.PinnedKeyPair != null && User.PinnedKeyPair.KeyPairId == KeyPairId;
            }
        }
    }
}