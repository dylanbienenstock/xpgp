using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace xpgp.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public IFormFile ProfilePicture { get; set; }

        [EmailAddress]
        public string PublicEmailAddress { get; set; }

        [MaxLength(196, ErrorMessage = "Bio must be less than 200 characters.")]
        public string Bio { get; set; }

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "Passwords must match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string NewPasswordConfirmation { get; set; }
    }
}