using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Account Login action method
    /// </summary>
    public class LoginViewModel
    {
        [Required]
        [MaxLength(20, ErrorMessage = "Maximum length is 20 characters!")]
        [MinLength(1, ErrorMessage = "Minimum length is 1 character!")]
        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,}$", ErrorMessage = "Password is not in correct format")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }
    }
}
