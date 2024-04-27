using NCloud.Users;

namespace NCloud.ViewModels
{
    public class AccountViewModel
    {
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }

        public AccountViewModel(string? username, string? fullName,string? email)
        {
            UserName = username;
            FullName = fullName;
            Email = email;
        }
    }
}
