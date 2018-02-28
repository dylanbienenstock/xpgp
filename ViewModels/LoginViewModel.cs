using System.ComponentModel.DataAnnotations;

namespace xpgp.ViewModels
{
	public class LoginViewModel : BaseViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}