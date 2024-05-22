using Microsoft.EntityFrameworkCore;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;

namespace CloudServicesTest
{
    [TestClass]
    public class CloudServiceTest : IDisposable
    {
        private readonly CloudService service;
        private readonly CloudDbContext context;
        private readonly CloudUser admin;
        private readonly CloudUser user;
        private readonly CloudUser user2;
        public CloudServiceTest()
        {
            var options = new DbContextOptionsBuilder<CloudDbContext>()
                                .UseInMemoryDatabase("NCloudTestDb")
                                .Options;

            context = new CloudDbContext(options);

            var users = TestDbAndFileSystemSeeder.SeedDb(context);

            admin = users.Item1;
            user = users.Item2;
            user2 = users.Item3;

            service = new CloudService(context);
        }
        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();

            string dir = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__");

            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }

        /// <summary>
        /// Test getting admin form database
        /// </summary>
        [TestMethod]
        public void GetAdminTestSuccess()
        {
            var res = service.GetAdmin().GetAwaiter().GetResult();

            Assert.IsTrue(res == admin);
        }

        /// <summary>
        /// Test removing user from database
        /// </summary>
        [TestMethod]
        public void RemoveUserTestSuccess()
        {
            var res = service.RemoveUser(user).GetAwaiter().GetResult();

            Assert.IsTrue(res);
            Assert.IsTrue(context.Users.Count() == 2);
        }

        /// <summary>
        /// Test for creating base directory for user
        /// </summary>
        [TestMethod]
        public void CreateBaseDirectoryForUserTestSuccess()
        {
            var res = service.CreateBaseDirectoryForUser(user2).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", user2.Id.ToString());

            foreach (string folder in Constants.SystemFolders)
            {
                Assert.IsTrue(Directory.Exists(Path.Combine(pathBase, folder)));
            }
        }

        /// <summary>
        /// Test for removing base directory for user
        /// </summary>
        [TestMethod]
        public void DeleteDirectoriesForUserTestSuccess()
        {
            service.CreateBaseDirectoryForUser(user2).GetAwaiter().GetResult();

            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", user2.Id.ToString());

            var res = service.DeleteDirectoriesForUser(pathBase, user2).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            Assert.IsFalse(Directory.Exists(pathBase));
        }
    }
}