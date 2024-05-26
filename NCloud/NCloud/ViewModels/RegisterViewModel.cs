using NCloud.ConstantData;
using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Account Register action method
    /// </summary>
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(20, ErrorMessage = "Maximum length is 20 characters!")]
        [MinLength(1, ErrorMessage = "Minimum length is 1 character!")]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [MaxLength(40, ErrorMessage = "Maximum length is 40 characters!")]
        [MinLength(1, ErrorMessage = "Minimum length is 1 character!")]
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Email is not in correct format")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&_])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password is not in correct format")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Password repeat is not in correct format")]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Repeat the password")]
        public string? PasswordRepeat { get; set; }
    }
}
