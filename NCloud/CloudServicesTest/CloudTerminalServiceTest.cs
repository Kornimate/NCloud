using Microsoft.EntityFrameworkCore;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;

namespace CloudServicesTest
{
    [TestClass]
    public class CloudTerminalServiceTest
    {
        private readonly CloudTerminalService service;
        private readonly CloudDbContext context;
        private readonly CloudUser admin;
        private readonly CloudUser user;
        private readonly CloudUser user2;
        public CloudTerminalServiceTest()
        {
            var options = new DbContextOptionsBuilder<CloudDbContext>()
                                .UseInMemoryDatabase("NCloudTestDb")
                                .Options;

            context = new CloudDbContext(options);

            var users = TestDbAndFileSystemSeeder.SeedDb(context);

            admin = users.Item1;
            user = users.Item2;
            user2 = users.Item3;

            service = new CloudTerminalService(new CloudService(context));
        }
        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();

            string dir = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__");

            if (Directory.Exists(dir))
                Directory.Delete(dir, true);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(true);
        }
    }
}