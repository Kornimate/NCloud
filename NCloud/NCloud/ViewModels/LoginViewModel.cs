using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [Display(Name = "Password")]
        public string? Password { get; set; }
    }
}
