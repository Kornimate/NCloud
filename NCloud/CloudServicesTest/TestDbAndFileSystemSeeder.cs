using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Users;

namespace CloudServicesTest
{
    internal class TestDbAndFileSystemSeeder
    {
        internal static CloudUser SeedDb(CloudDbContext context)
        {
            context.Database.EnsureCreated();

            var admin = new CloudUser(Constants.AdminUserName, Constants.AdminUserName);

            context.Users.Add(admin);

            context.SaveChanges();

            return admin;
        }
    }
}