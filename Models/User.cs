using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xpgp.Models
{
	public class User : BaseModel
    {
        public User()
        {
            KeyPairs = new List<KeyPair>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [MaxLength(64)]
        public string FirstName { get; set; }

        [MaxLength(64)]
        public string LastName { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        [EmailAddress]
        public string PublicEmailAddress { get; set; }

        [MaxLength(196)]
        public string Bio { get; set; }

        public byte[] ProfilePicture { get; set; }

        public string PasswordHash { get; set; }

        public string LoginToken { get; set; }

        public List<KeyPair> KeyPairs { get; set; }

        public KeyPair PinnedKeyPair { get; set; }

        public int? PinnedKeyPairId { get; set; }
    }
}