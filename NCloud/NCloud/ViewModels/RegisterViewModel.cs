using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string? UserName { get; set; }
        [Required] // need extra check
        public string? FullName { get; set; }
        [Required] // need extra check
        public string? Password { get; set; }
        [Required] // need extra check
        public string? PasswordRepeat { get; set; }
    }
}
