using Microsoft.EntityFrameworkCore;
using NCloud.Models;
using NCloud.Services;

namespace CloudServicesTest
{
    [TestClass]
    public class CloudTerminalServiceTest
    {
        private readonly CloudService service;
        private readonly CloudDbContext context;
        public CloudTerminalServiceTest()
        {
            var options = new DbContextOptionsBuilder<CloudDbContext>()
                                .UseInMemoryDatabase("NCloudTestDb")
                                .Options;

            context = new CloudDbContext(options);

            service = new CloudService(context);

            //Data initialization
        }

        public void Dispose()
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(true);
        }
    }
}