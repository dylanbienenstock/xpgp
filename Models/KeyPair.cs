using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace xpgp.Models
{
    public class KeyPair : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KeypairId { get; set; }

        public string Name;

        [MaxLength(1024)]
        public byte[] PublicKey { get; set; }
 
        [MaxLength(1024)]
        public byte[] PrivateKey { get; set; }

        public User User { get; set; }
    }
}