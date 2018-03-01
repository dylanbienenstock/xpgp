using System.ComponentModel.DataAnnotations;

namespace xpgp.ViewModels
{
    public class KeyPairViewModel : BaseViewModel
    {
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(45)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [MaxLength(45)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        
        [Required]
        public int Expiration { get; set; }

        [Required]
        public string ExpirationUnits { get; set; }
    }
}