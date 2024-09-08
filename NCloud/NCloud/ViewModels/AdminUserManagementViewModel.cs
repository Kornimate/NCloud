using NCloud.Users;

namespace NCloud.ViewModels
{
    public class AdminUserManagementViewModel
    {
        public List<CloudUser> Users { get; set; }

        public AdminUserManagementViewModel(List<CloudUser> users)
        {
            Users = users;
        }

        public AdminUserManagementViewModel()
        {
            Users = new();
        }
    }
}
