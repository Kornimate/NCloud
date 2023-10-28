using NCloud.Users;

namespace NCloud.ViewModels
{
    public class AccountViewModel
    {
        public string? UserName { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }

        public AccountViewModel(string? username, string? fullname, string? email)
        {
            UserName = username;
            Fullname = fullname;
            Email = email;
        }
    }
}
