using NCloud.Users;

namespace NCloud.ViewModels
{
    public class AccountViewModel
    {
        public string? UserName { get; set; }

        public AccountViewModel(string? username)
        {
            UserName = username;
        }
    }
}
