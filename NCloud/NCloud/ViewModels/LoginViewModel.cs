using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    /// <summary>
    /// Container class for data in Account Login action method
    /// </summary>
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
