using Microsoft.EntityFrameworkCore;
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
        public CloudServiceTest()
        {
            var options = new DbContextOptionsBuilder<CloudDbContext>()
                                .UseInMemoryDatabase("NCloudTestDb")
                                .Options;

            context = new CloudDbContext(options);

            admin = TestDbAndFileSystemSeeder.SeedDb(context);

            service = new CloudService(context);

            //Data initialization
        }
        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        /// <summary>
        /// Test method to test getting admin form database
        /// </summary>
        [TestMethod]
        public void GetAdminTestSuccess()
        {
            var res = service.GetAdmin().GetAwaiter().GetResult();

            Assert.IsTrue(res == admin);
        }

        [TestMethod]
        public void ()
        {
            var res = service.GetAdmin().GetAwaiter().GetResult();

        Assert.IsTrue(res == admin);
        }
}
}