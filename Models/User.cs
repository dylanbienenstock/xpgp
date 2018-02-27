using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xpgp.Models
{
	public class User : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [StringLength(64)]
        public string FirstName { get; set; }

        [StringLength(64)]
        public string LastName { get; set; }

        [EmailAddress]
        public string EmailAddress { get; set; }

        public string PasswordHash { get; set; }

        public string LoginToken { get; set; }

        public List<KeyPair> KeyPairs { get; set; }
    }
}