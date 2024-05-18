using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Account Register action method
    /// </summary>
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(10,ErrorMessage ="Maximum length is 10 chars!")]
        [MinLength(1,ErrorMessage ="Minimum length is 10 chars!")]
        [Display(Name = "Username")]
        public string? UserName { get; set; }
        [Required] // need extra check
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required] // need extra check
        [Display(Name = "Password")]
        public string? Password { get; set; }
        [Required] // need extra check
        [Display(Name = "Repeat the password")]
        public string? PasswordRepeat { get; set; }
    }
}
