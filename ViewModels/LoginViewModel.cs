using System.ComponentModel.DataAnnotations;

namespace xpgp.ViewModels
{
	public class LoginViewModel : BaseViewModel
    {
        [Required(ErrorMessage = "* Required")]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "* Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}