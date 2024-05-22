using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Users;

namespace CloudServicesTest
{
    internal class TestDbAndFileSystemSeeder
    {
        internal static (CloudUser, CloudUser, CloudUser) SeedDb(CloudDbContext context)
        {
            context.Database.EnsureCreated();

            var admin = new CloudUser(Constants.AdminUserName, Constants.AdminUserName);
            var user = new CloudUser("User", "User");
            var user2 = new CloudUser("User2", "User2");

            context.Users.Add(admin);
            context.Users.Add(user);
            context.Users.Add(user2);

            context.SaveChanges();

            return (admin, user, user2);
        }
    }
}