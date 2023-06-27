using System.ComponentModel.DataAnnotations;

namespace NCloud.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [MaxLength(10,ErrorMessage ="Maximum length is 10 chars!")]
        [MinLength(1,ErrorMessage ="Minimum length is 10 chars!")]
        //regex for names
        public string? UserName { get; set; }
        [Required] // need extra check
        public string? FullName { get; set; }
        [Required] // need extra check
        public string? Password { get; set; }
        [Required] // need extra check
        public string? PasswordRepeat { get; set; }
    }
}
