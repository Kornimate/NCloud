using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;

namespace CloudServicesTest
{
    [TestClass]
    public class CloudTerminalServiceTest
    {
        private readonly CloudTerminalService service;
        private readonly CloudDbContext context;
        private readonly CloudUser admin;

        public CloudTerminalServiceTest()
        {
            var options = new DbContextOptionsBuilder<CloudDbContext>()
                                .UseInMemoryDatabase("NCloudTestDb")
                                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                                .Options;

            context = new CloudDbContext(options);

            var users = TestDbAndFileSystemSeeder.SeedDb(context);

            admin = users.Item1;

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

        /// <summary>
        /// Test method to execute terminal command
        /// </summary>
        [TestMethod]
        public void ExecuteTestSuccess()
        {
            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            SharedPathData sharedData = new SharedPathData();

            var resp = service.Execute("pwd", new List<string>(), data, sharedData, admin).GetAwaiter().GetResult();

            Assert.IsTrue(resp.Item1);
            Assert.IsNotNull(resp.Item3);
            Assert.IsTrue(resp.Item3 is string);
            Assert.AreEqual(resp.Item3.ToString()?.Trim(), "@CLOUDROOT");
        }

        /// <summary>
        /// Test method for execute terminal command errors
        /// </summary>
        [TestMethod]
        public void ExecuteTestErrors()
        {
            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            SharedPathData sharedData = new SharedPathData();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.Execute("", new List<string>() { "--------" }, data, sharedData, admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.Execute("mkdir", new List<string>() { "--------" }, data, sharedData, admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.Execute("mkdir", new List<string>() { "--------", "---------" }, data, sharedData, admin).GetAwaiter().GetResult());

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents"));

            var resp = service.Execute("rm-dir", new List<string>() { "Documents" }, data, sharedData, admin).GetAwaiter().GetResult();

            Assert.IsFalse(resp.Item1);
        }

        /// <summary>
        /// Test to get server side commands
        /// </summary>
        [TestMethod]
        public void ServerSideCommandsTest()
        {
            var resp = service.GetServerSideCommands();

            Assert.IsTrue(resp.Count > 0);
        }

        /// <summary>
        /// Test to get client side commands
        /// </summary>
        [TestMethod]
        public void ClientSideCommandsTest()
        {
            var resp = service.GetClientSideCommands();

            Assert.IsTrue(resp.Count > 0);
        }

        /// <summary>
        /// Test to get client side commands (as objects)
        /// </summary>
        [TestMethod]
        public void ClientSideCommandObjectsTest()
        {
            var resp = service.GetClientSideCommandsObjectList();

            Assert.IsTrue(resp.Count > 0);
        }

        /// <summary>
        /// Test to get commands
        /// </summary>
        [TestMethod]
        public void CommandsTest()
        {
            var resp = service.GetCommands();
            var client = service.GetClientSideCommands();
            var server = service.GetServerSideCommands();

            Assert.IsTrue(resp.Count > 0);
            Assert.AreEqual(resp.Count, client.Count + server.Count);
        }

        /// <summary>
        /// Test methdo to get command url for client side commands
        /// </summary>
        [TestMethod]
        public void GetCommandUrlTestSuccess()
        {
            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            var resp = service.GetClientSideCommandUrlDetails("edit", new List<string>() { "Test.txt" }, data).GetAwaiter().GetResult();

            Assert.AreEqual("Editor", resp.Controller);
            Assert.AreEqual("EditorHub", resp.Action);
        }

        /// <summary>
        /// Test methdo for get command url for client side commands errors
        /// </summary>
        [TestMethod]
        public void GetCommandUrlTestErrors()
        {
            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetClientSideCommandUrlDetails("wrong-command", new List<string>() { "Test.txt" }, data).GetAwaiter().GetResult());
        }
    }
}