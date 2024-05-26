using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.Users.Roles;
using System.IO;
using System.Xml.Linq;

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
            service.CreateBaseDirectoryForUser(user2).Wait();

            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", user2.Id.ToString());

            var res = service.DeleteDirectoriesForUser(pathBase, user2).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            Assert.IsFalse(Directory.Exists(pathBase));
        }

        /// <summary>
        /// Test for user adding directory
        /// </summary>
        [TestMethod]
        public void AddDirectoryTestSuccess()
        {
            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            service.CreateBaseDirectoryForUser(admin).Wait();

            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents");

            string dirName = "Test";

            string res = service.CreateDirectory(dirName, $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult();

            Assert.AreEqual(res, dirName);

            Assert.IsTrue(Directory.Exists(Path.Combine(pathBase, dirName)));
        }

        /// <summary>
        /// Test for user adding directory errors
        /// </summary>
        [TestMethod]
        public void AddDirectoryTestErrors()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString());

            string dirName = "Test";

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateDirectory(null!, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateDirectory(dirName, null!, admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateDirectory("######", $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());

            service.CreateDirectory(dirName, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateDirectory(dirName, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test for user removing directory
        /// </summary>
        [TestMethod]
        public void RemoveDirectoryTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string dirName = "Test";
            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString());

            service.CreateDirectory(dirName, $"@CLOUDROOT\\{admin.Id}", admin).Wait();

            bool res = service.RemoveDirectory(dirName, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            Assert.IsFalse(Directory.Exists(Path.Combine(pathBase, dirName)));
        }

        /// <summary>
        /// Test for user removing directory errors
        /// </summary>
        [TestMethod]
        public void RemoveDirectoryTestErrors()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string dirName = "Test";

            Assert.ThrowsException<CloudFunctionStopException>(() => service.RemoveDirectory(null!, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.RemoveDirectory(dirName, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
            Assert.IsFalse(service.RemoveDirectory("Documents", $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test for user adding file to user storage
        /// </summary>
        [TestMethod]
        public void AddingFileTestSuccess()
        {
            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";
            string name2 = "Test_1.txt";
            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents");

            IFormFile file;
            IFormFile file2;

            using (var stream = File.OpenRead(name))
            {

                file = new FormFile(stream, 0, stream.Length, null!, name)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                };

                string res = service.CreateFile(file, $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult();

                Assert.AreEqual(res, name);

                Assert.IsTrue(File.Exists(Path.Combine(pathBase, name)));
            }

            using (var stream = File.OpenRead(name))
            {

                file2 = new FormFile(stream, 0, stream.Length, null!, name)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                };

                string res = service.CreateFile(file2, $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult();

                Assert.AreEqual(name2, res);

                Assert.IsTrue(File.Exists(Path.Combine(pathBase, name2)));
            }
        }

        /// <summary>
        /// Test for user adding file to user storage errors
        /// </summary>
        [TestMethod]
        public void AddingFileTestErrors()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            IFormFile file;

            using (var stream = File.OpenRead(name))
            {

                file = new FormFile(stream, 0, stream.Length, null!, name)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                };
            }

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateFile(file, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateFile(null!, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateFile(file, null!, admin).GetAwaiter().GetResult());

            admin.UsedSpace = Constants.UserSpaceSize;

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CreateFile(new FormFile(new MemoryStream(), 0, 0, null!, "#####"), $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test for user removing file to user storage
        /// </summary>
        [TestMethod]
        public void RemoveFileTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";
            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString());

            IFormFile file;

            using (var stream = File.OpenRead(name))
            {

                file = new FormFile(stream, 0, stream.Length, null!, name)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                };

                service.CreateFile(file, $"@CLOUDROOT\\{admin.Id}", admin).Wait();

            }

            bool res = service.RemoveFile(name, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            Assert.IsFalse(File.Exists(Path.Combine(pathBase, name)));
        }

        /// <summary>
        /// Test for user removing file from user storage errors
        /// </summary>
        [TestMethod]
        public void RemoveFileTestErrors()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            Assert.ThrowsException<CloudFunctionStopException>(() => service.RemoveFile(null!, $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.RemoveFile(name, null!, admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.RemoveFile("WrongName", $"@CLOUDROOT\\{admin.Id}", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test for user to get current state files
        /// </summary>
        [TestMethod]
        public void GetCurrentDepthFilesTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            IFormFile file;

            using (var stream = File.OpenRead(name))
            {

                file = new FormFile(stream, 0, stream.Length, null!, name)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "text/plain"
                };

                service.CreateFile(file, $"@CLOUDROOT\\{admin.Id}", admin).Wait();

            }

            context.Add(new SharedFile
            {
                Id = Guid.NewGuid(),
                Name = name,
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAEDROOT\\{admin.UserName}",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = false
            });

            context.SaveChanges();

            List<CloudFile> res = service.GetCurrentDepthCloudFiles($"@CLOUDROOT\\{admin.Id}").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            res = service.GetCurrentDepthCloudFiles($"@CLOUDROOT\\{admin.Id}", connectedToApp: true).GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            res = service.GetCurrentDepthCloudFiles($"@CLOUDROOT\\{admin.Id}", connectedToWeb: true).GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.GetCurrentDepthCloudFiles($"@CLOUDROOT\\{admin.Id}", pattern: "*.txt").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            res = service.GetCurrentDepthCloudFiles($"@CLOUDROOT\\{admin.Id}", pattern: "######").GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);
        }

        /// <summary>
        /// Test for user to get current state files errors
        /// </summary>
        [TestMethod]
        public void GetCurrentDepthFilesTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetCurrentDepthCloudFiles($"@CLOUDROOT\\{admin.Id}\\Wrong\\Path").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test for user to get current state folders
        /// </summary>
        [TestMethod]
        public void GetCurrentDepthDirectoriesTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Documents";

            context.Add(new SharedFolder
            {
                Id = Guid.NewGuid(),
                Name = name,
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAEDROOT\\{admin.UserName}",
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            List<CloudFolder> res = service.GetCurrentDepthCloudDirectories($"@CLOUDROOT\\{admin.Id}").GetAwaiter().GetResult();

            Assert.AreEqual(4, res.Count);

            res = service.GetCurrentDepthCloudDirectories($"@CLOUDROOT\\{admin.Id}", connectedToApp: true).GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.GetCurrentDepthCloudDirectories($"@CLOUDROOT\\{admin.Id}", connectedToWeb: true).GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            res = service.GetCurrentDepthCloudDirectories($"@CLOUDROOT\\{admin.Id}", pattern: "*i*").GetAwaiter().GetResult();

            Assert.AreEqual(3, res.Count);

            res = service.GetCurrentDepthCloudDirectories($"@CLOUDROOT\\{admin.Id}", pattern: "######").GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);
        }

        /// <summary>
        /// Test for user to get current state folders errors
        /// </summary>
        [TestMethod]
        public void GetCurrentDepthDirectoriesTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetCurrentDepthCloudDirectories($"@CLOUDROOT\\{admin.Id}\\Wrong\\Path").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test root name changing protocol
        /// </summary>
        [TestMethod]
        public void ChangeRootNameTest()
        {
            string path = $"@CLOUDROOT\\{admin.Id}";
            string path2 = $"@SHAREDROOT\\{admin.Id}";
            string path3 = "##############";

            string res = service.ChangeRootName(path);

            Assert.AreEqual(res, path2);

            res = service.ChangeRootName(path2);

            Assert.AreEqual(res, path);

            res = service.ChangeRootName(path3);

            Assert.AreEqual(res, path3);
        }

        /// <summary>
        /// Method to test cloud path to physical path converting
        /// </summary>
        [TestMethod]
        public void ServerPathTest()
        {
            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private");

            string res = service.ServerPath("@CLOUDROOT");

            Assert.AreEqual(res, pathBase);

            Assert.AreEqual(String.Empty, service.ServerPath("cbusuzcuszvucduvcud"));
        }

        /// <summary>
        /// Method to test getting folder data based on path and name
        /// </summary>
        [TestMethod]
        public void GetFolderByPathTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Documents";
            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString());

            DirectoryInfo di = service.GetFolderByPath(pathBase, name).GetAwaiter().GetResult();

            Assert.IsTrue(di.Exists);
            Assert.AreEqual(name, di.Name);
            Assert.AreEqual(Path.Combine(pathBase, name), di.FullName);
        }

        /// <summary>
        /// Method to test getting folder data based on path and name
        /// </summary>
        [TestMethod]
        public void GetFolderByPathTestErrors()
        {
            string name = "Documents";
            string pathBase = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name);

            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetFolderByPath(pathBase, name).GetAwaiter().GetResult());

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetFolderByPath(pathBase, name).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test connecting directory to web
        /// </summary>
        [TestMethod]
        public void ConnectDirectoryToWebTestSucces()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            bool res = service.ConnectDirectoryToWeb(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFolder? f = context.SharedFolders.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNotNull(f);

            Assert.IsTrue(f.ConnectedToWeb);
        }

        /// <summary>
        /// Method to test connecting directory to web errors
        /// </summary>
        [TestMethod]
        public void ConnectDirectoryToWebTestErrors()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.ConnectDirectoryToWeb(pathBase, "######", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.ConnectDirectoryToWeb(Path.Combine(pathBase, name), name, admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test connecting directory to app
        /// </summary>
        [TestMethod]
        public void ConnectDirectoryToAppTestSucces()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            bool res = service.ConnectDirectoryToApp(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFolder? f = context.SharedFolders.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNotNull(f);

            Assert.IsTrue(f.ConnectedToApp);
        }

        /// <summary>
        /// Method to test connecting directory to app errors
        /// </summary>
        [TestMethod]
        public void ConnectDirectoryToAppTestErrors()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.ConnectDirectoryToApp(pathBase, "######", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.ConnectDirectoryToApp(Path.Combine(pathBase, name), name, admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test disconnecting directory from web
        /// </summary>
        [TestMethod]
        public void DisconnectDirectoryFromWebTestSucces()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            service.ConnectDirectoryToWeb(pathBase, name, admin).Wait();

            bool res = service.DisconnectDirectoryFromWeb(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFolder? f = context.SharedFolders.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNull(f);
        }

        /// <summary>
        /// Method to test disconnecting directory from web errors
        /// </summary>
        [TestMethod]
        public void DisconnectDirectoryFromWebTestErrors()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.DisconnectDirectoryFromWeb(pathBase, "######", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.DisconnectDirectoryFromWeb(Path.Combine(pathBase, name), name, admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test connecting directory to web
        /// </summary>
        [TestMethod]
        public void DisconnectDirectoryFromAppTestSucces()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            service.ConnectDirectoryToApp(pathBase, name, admin).Wait();

            bool res = service.DisconnectDirectoryFromApp(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFolder? f = context.SharedFolders.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNull(f);
        }

        /// <summary>
        /// Method to test disconnecting directory from web errors
        /// </summary>
        [TestMethod]
        public void DisconnectDirectoryFromAppTestErrors()
        {
            string name = "Documents";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.DisconnectDirectoryFromApp(pathBase, "######", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.DisconnectDirectoryFromApp(Path.Combine(pathBase, name), name, admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test connecting file to web
        /// </summary>
        [TestMethod]
        public void ConnectFileToWebTestSucces()
        {
            string name = "Test.txt";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            bool res = service.ConnectFileToWeb(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFile? f = context.SharedFiles.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNotNull(f);

            Assert.IsTrue(f.ConnectedToWeb);
        }

        /// <summary>
        /// Method to test connecting file to web errors
        /// </summary>
        [TestMethod]
        public void ConnectFileToWebTestErrors()
        {
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.ConnectFileToWeb(pathBase, "######", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test connecting file to app
        /// </summary>
        [TestMethod]
        public void ConnectFileToAppTestSucces()
        {
            string name = "Test.txt";
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            bool res = service.ConnectFileToApp(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFile? f = context.SharedFiles.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNotNull(f);

            Assert.IsTrue(f.ConnectedToApp);
        }

        /// <summary>
        /// Method to test connecting file to app errors
        /// </summary>
        [TestMethod]
        public void ConnectFileToAppTestErrors()
        {
            string pathBase = $"@CLOUDROOT\\{admin.Id}";

            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.ConnectFileToApp(pathBase, "######", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Method to test disconnecting file from web
        /// </summary>
        [TestMethod]
        public void DisconnectFileFromWebTestSucces()
        {
            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}\\Documents",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}\\Documents",
                Name = "Test.txt",
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            string name = "Test.txt";
            string pathBase = $"@CLOUDROOT\\{admin.Id}\\Documents";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            bool res = service.DisconnectFileFromWeb(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFile? f = context.SharedFiles.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNull(f);
        }

        /// <summary>
        /// Method to test disconnecting file from app
        /// </summary>
        [TestMethod]
        public void DisconnectFileFromAppTestSucces()
        {
            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = false
            });

            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}\\Documents",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}\\Documents",
                Name = "Test.txt",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = false
            });

            context.SaveChanges();

            string name = "Test.txt";
            string pathBase = $"@CLOUDROOT\\{admin.Id}\\Documents";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            bool res = service.DisconnectFileFromApp(pathBase, name, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            SharedFile? f = context.SharedFiles.FirstOrDefault(x => x.CloudPathFromRoot == pathBase && x.Name == name);

            Assert.IsNull(f);

            Assert.IsTrue(!context.SharedFolders.Any());
        }


        /// <summary>
        /// Test mthod to get current depth sharing files
        /// </summary>
        [TestMethod]
        public void CurrentAppSharingFilesTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = name,
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = false
            });

            context.SaveChanges();

            var res = service.GetCurrentDepthAppSharingFiles("@SHAREDROOT").GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.GetCurrentDepthAppSharingFiles($"@SHAREDROOT\\{admin.UserName}").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
        }

        /// <summary>
        /// Test mthod to get current depth sharing folders
        /// </summary>
        [TestMethod]
        public void CurrentAppSharingDirectoriesTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = false
            });

            context.SaveChanges();

            var res = service.GetCurrentDepthAppSharingDirectories("@SHAREDROOT").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
            Assert.AreEqual(res[0].SharedName, admin.UserName);

            res = service.GetCurrentDepthAppSharingDirectories($"@SHAREDROOT\\{admin.UserName}").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
        }

        /// <summary>
        /// Test method if owner of path is actual user
        /// </summary>
        [TestMethod]
        public void OwnerOfPathIsActualUserTest()
        {
            string path = "@SHAREDROOT\\Admin1";

            bool res = service.OwnerOfPathIsActualUser(path, admin).GetAwaiter().GetResult();

            Assert.IsFalse(res);

            path = "@SHAREDROOT\\Admin";

            res = service.OwnerOfPathIsActualUser(path, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);
        }

        /// <summary>
        /// Test mthod to get current user web sharing files
        /// </summary>
        [TestMethod]
        public void UserWebSharedFilesTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = name,
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var res = service.GetUserWebSharedFiles(user).GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.GetUserWebSharedFiles(admin).GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
        }

        /// <summary>
        /// Test mthod to get current user web sharing folders
        /// </summary>
        [TestMethod]
        public void UserWebSharedFoldersTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var res = service.GetUserWebSharedFolders(user).GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.GetUserWebSharedFolders(admin).GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
        }

        [TestMethod]
        public void WebBackCheckTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            string path = $"@CLOUDROOT\\{admin.Id}\\Documents\\Test";

            string res = service.WebBackCheck(path).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT\\{admin.Id}\\Documents", res);

            path = $"@CLOUDROOT\\{admin.Id}\\Documents\\Test\\TestII\\TestIII";

            res = service.WebBackCheck(path).GetAwaiter().GetResult();

            Assert.AreEqual(res, path);
        }

        /// <summary>
        /// Test mthod to get current depth web sharing files
        /// </summary>
        [TestMethod]
        public void WebCurrentDepthSharedFilesTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents"));

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents", name));

            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}\\Documents",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}\\Documents",
                Name = name,
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            string path = $"@CLOUDROOT\\{admin.Id}\\Documents";


            var res = service.GetCurrentDepthWebSharingFiles(path).GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            path = $"@CLOUDROOT\\{admin.Id}\\Documents\\Test";

            res = service.GetCurrentDepthWebSharingFiles(path).GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);
        }

        /// <summary>
        /// Test mthod to get current depth web sharing folders
        /// </summary>
        [TestMethod]
        public void WebCurrentDepthSharedFoldersTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = false,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            string path = $"@CLOUDROOT\\{admin.Id}";

            var res = service.GetCurrentDepthWebSharingDirectories(path).GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            path = $"@CLOUDROOT\\{admin.Id}\\Documentst";

            res = service.GetCurrentDepthWebSharingDirectories(path).GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);
        }

        /// <summary>
        /// Test method to change current state to sharing state and sharing state to current state
        /// </summary>
        [TestMethod]
        public void ChangePathStructureTest()
        {
            string path = $"@CLOUDROOT";

            string res = service.ChangePathStructure(path).GetAwaiter().GetResult();

            Assert.AreEqual(res, path);

            path = $"@CLOUDROOT\\{admin.Id}";

            res = service.ChangePathStructure(path).GetAwaiter().GetResult();

            Assert.AreEqual($"@SHAREDROOT\\{admin.UserName}", res);

            path = $"@CLOUDROOT\\testnotworkingID";

            res = service.ChangePathStructure(path).GetAwaiter().GetResult();

            Assert.AreEqual(path, res);

            path = $"@SHAREDROOT\\{admin.UserName}";

            res = service.ChangePathStructure(path).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT\\{admin.Id}", res);

            path = $"@CLOUDROOT\\testnotworkingID";

            res = service.ChangePathStructure(path).GetAwaiter().GetResult();

            Assert.AreEqual(path, res);
        }


        /// <summary>
        /// Test method to modify file content to given content
        /// </summary>
        [TestMethod]
        public void ModifyFileContentTestSuccess()
        {
            string content = "This is a test content";
            string name = "Test.txt";
            string pathToFile = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name);

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, pathToFile);

            bool res = service.ModifyFileContent($"@CLOUDROOT\\{admin.Id}\\Test.txt", content, admin).GetAwaiter().GetResult();

            Assert.IsTrue(res);

            Assert.AreEqual(content, File.ReadAllText(pathToFile));
        }

        /// <summary>
        /// Test method to modify file content to given content errors
        /// </summary>
        [TestMethod]
        public void ModifyFileContentTestErrors()
        {
            string content = "This is a test content but for actually long because the default text is long in the file too.";
            string name = "Test.txt";
            string pathToFile = Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name);

            Assert.ThrowsException<FileNotFoundException>(() => service.ModifyFileContent($"@CLOUDROOT\\{admin.Id}\\Test.txt", content, admin).GetAwaiter().GetResult());

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, pathToFile);

            admin.UsedSpace = Constants.UserSpaceSize;

            Assert.ThrowsException<CloudFunctionStopException>(() => service.ModifyFileContent($"@CLOUDROOT\\{admin.Id}\\Test.txt", content, admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to get folder by path and name
        /// </summary>
        [TestMethod]
        public void GetFolderTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var res = service.GetFolder($"@CLOUDROOT\\{admin.Id}", "Documents").GetAwaiter().GetResult();

            Assert.IsNotNull(res);
            Assert.IsTrue(res.Info.Exists);
            Assert.IsTrue(res.IsConnectedToWeb);
            Assert.IsTrue(res.IsConnectedToApp);
        }

        /// <summary>
        /// Test method to get file by path and name
        /// </summary>
        [TestMethod]
        public void GetFileTest()
        {
            string name = "Test.txt";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = name,
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var res = service.GetFile($"@CLOUDROOT\\{admin.Id}", name).GetAwaiter().GetResult();

            Assert.IsNotNull(res);
            Assert.IsTrue(res.Info.Exists);
            Assert.IsTrue(res.IsConnectedToWeb);
            Assert.IsTrue(res.IsConnectedToApp);
        }

        /// <summary>
        /// Test method to rename folder
        /// </summary>
        [TestMethod]
        public void RenameFolderTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            service.CreateDirectory("Test", $"@CLOUDROOT\\{admin.Id}", admin).Wait();

            var res = service.RenameFolder($"@CLOUDROOT\\{admin.Id}", "Test", "TestII", new SharedPathData()).GetAwaiter().GetResult();

            Assert.AreEqual(res, "TestII");
        }

        /// <summary>
        /// Test method for rename folder errors
        /// </summary>
        [TestMethod]
        public void RenameFolderTestErrors()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.RenameFolder($"@CLOUDROOT\\{admin.Id}", "Test", "TestII", new SharedPathData()).GetAwaiter().GetResult());

            service.CreateDirectory("Test", $"@CLOUDROOT\\{admin.Id}", admin).Wait();

            Assert.ThrowsException<CloudFunctionStopException>(() => service.RenameFolder($"@CLOUDROOT\\{admin.Id}", "Documents", "TestII", new SharedPathData()).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.RenameFolder($"@CLOUDROOT\\{admin.Id}", "Test", "Documents", new SharedPathData()).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to rename file
        /// </summary>
        [TestMethod]
        public void RenameFileTest()
        {
            string name = "Test.txt";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));
            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "TestIII.txt"));

            var res = service.RenameFile($"@CLOUDROOT\\{admin.Id}", name, "TestII.txt").GetAwaiter().GetResult();

            Assert.AreEqual(res, "TestII.txt");

            name = res;

            res = service.RenameFile($"@CLOUDROOT\\{admin.Id}", name, "TestIII.txt").GetAwaiter().GetResult();

            Assert.AreEqual(res, "TestIII_1.txt");
        }

        /// <summary>
        /// Test method for rename file errors
        /// </summary>
        [TestMethod]
        public void RenameFileTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.RenameFile($"@CLOUDROOT\\{admin.Id}", "Test.txt", "TestI.txt").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to copy file
        /// </summary>
        [TestMethod]
        public void CopyFileTestSuccess()
        {
            string name = "Test.txt";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            var res = service.CopyFile($"@CLOUDROOT\\{admin.Id}\\{name}", $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult();

            Assert.AreEqual(res, String.Empty);
            Assert.IsTrue(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents", name)));

            res = service.CopyFile($"@CLOUDROOT\\{admin.Id}\\{name}", $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult();

            Assert.AreEqual(res, "Test_1.txt");
        }

        /// <summary>
        /// Test method for copy file errors
        /// </summary>
        [TestMethod]
        public void CopyFileTestErrors()
        {
            string name = "Test.txt";

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFile(null!, $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFile($"@CLOUDROOT\\{admin.Id}\\{name}", null!, admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFile($"@CLOUDROOT\\{admin.Id}\\{name}_error", $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFile($"@CLOUDROOT\\{admin.Id}\\{name}", $"@CLOUDROOT\\{admin.Id}\\WrongPath", admin).GetAwaiter().GetResult());


            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            admin.UsedSpace = Constants.UserSpaceSize;

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFile($"@CLOUDROOT\\{admin.Id}\\{name}", $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to copy folder
        /// </summary>
        [TestMethod]
        public void CopyFolderTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            var res = service.CopyFolder($"@CLOUDROOT\\{admin.Id}\\Documents", $"@CLOUDROOT\\{admin.Id}\\Music", admin).GetAwaiter().GetResult();

            Assert.AreEqual(res, String.Empty);
            Assert.IsTrue(Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Music", "Documents")));

            res = service.CopyFolder($"@CLOUDROOT\\{admin.Id}\\Documents", $"@CLOUDROOT\\{admin.Id}\\Music", admin).GetAwaiter().GetResult();

            Assert.AreEqual(res, "Documents_1");
        }

        /// <summary>
        /// Test method for copy folder errors
        /// </summary>
        [TestMethod]
        public void CopyFolderTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFolder(null!, $"@CLOUDROOT\\{admin.Id}\\Music", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFolder($"@CLOUDROOT\\{admin.Id}\\Documents", null!, admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFolder($"@CLOUDROOT\\{admin.Id}\\Documents_error", $"@CLOUDROOT\\{admin.Id}\\Documents", admin).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFolder($"@CLOUDROOT\\{admin.Id}\\Documents", $"@CLOUDROOT\\{admin.Id}\\WrongPath", admin).GetAwaiter().GetResult());

            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), "Documents", name));

            admin.UsedSpace = Constants.UserSpaceSize;

            Assert.ThrowsException<CloudFunctionStopException>(() => service.CopyFolder($"@CLOUDROOT\\{admin.Id}\\Documents", $"@CLOUDROOT\\{admin.Id}\\Music", admin).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to change directory
        /// </summary>
        [TestMethod]
        public void ChangeToDirectoryTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            var res = service.ChangeToDirectory(String.Empty, data).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT\\{admin.Id}", res.CurrentPath);

            res = service.ChangeToDirectory("Documents", data).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT\\{admin.Id}\\Documents", res.CurrentPath);

            res = service.ChangeToDirectory("..", data).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT\\{admin.Id}", res.CurrentPath);

            res = service.ChangeToDirectory($"@CLOUDROOT\\{admin.Id}\\Music", data).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT\\{admin.Id}\\Music", res.CurrentPath);
        }

        /// <summary>
        /// Test method for change directory errors
        /// </summary>
        [TestMethod]
        public void ChangeToDirectoryTestErrors()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            Assert.ThrowsException<CloudFunctionStopException>(() => service.ChangeToDirectory($"@CLOUDROOT\\{admin.Id}\\Test", data).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.ChangeToDirectory($"Documents\\", data).GetAwaiter().GetResult());
            Assert.ThrowsException<CloudFunctionStopException>(() => service.ChangeToDirectory("Test", data).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to list subdirectories
        /// </summary>
        [TestMethod]
        public void ListSubdirectoriesTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            var res = service.ListCurrentSubDirectories(data).GetAwaiter().GetResult();

            Assert.IsTrue(res != Constants.TerminalRedText("error while printing items"));
            Assert.IsTrue(res.Length > 0);
        }

        /// <summary>
        /// Test method to get terminal help text
        /// </summary>
        [TestMethod]
        public void GetTerminalHelpTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            var res = service.GetTerminalHelpText().GetAwaiter().GetResult();

            Assert.IsTrue(res != Constants.TerminalRedText("error while loading help"));
            Assert.IsTrue(res.Length > 0);
        }

        /// <summary>
        /// Test method to print working directory
        /// </summary>
        [TestMethod]
        public void PrintWorkingDirectoryTest()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            CloudPathData data = new CloudPathData();

            data.SetDefaultPathData(admin.Id.ToString());

            var res = service.PrintWorkingDirectory(data).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT", res.Trim());

            data = service.ChangeToDirectory("Documents", data).GetAwaiter().GetResult();

            res = service.PrintWorkingDirectory(data).GetAwaiter().GetResult();

            Assert.AreEqual($"@CLOUDROOT/Documents", res.Trim());
        }

        /// <summary>
        /// Test for user to get current state folders filtered by search pattern
        /// </summary>
        [TestMethod]
        public void SearchDirectoryInCurrentStateTestSuccess()
        {
            service.CreateBaseDirectoryForUser(admin).Wait();

            List<CloudFolder> res = service.SearchDirectoryInCurrentDirectory($"@CLOUDROOT\\{admin.Id}","*").GetAwaiter().GetResult();

            Assert.AreEqual(4, res.Count);

            res = service.SearchDirectoryInCurrentDirectory($"@CLOUDROOT\\{admin.Id}","#######").GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.SearchDirectoryInCurrentDirectory($"@CLOUDROOT\\{admin.Id}","*ocu*").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
        }

        /// <summary>
        /// Test for user for get current state folders filtered by search pattern errors
        /// </summary>
        [TestMethod]
        public void SearchDirectoryInCurrentStateTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.SearchDirectoryInCurrentDirectory($"@CLOUDROOT\\{admin.Id}\\Test","*").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test for user to get current state files filtered by search pattern
        /// </summary>
        [TestMethod]
        public void SearchFileInCurrentStateTestSuccess()
        {
            string name = "Test.txt";

            service.CreateBaseDirectoryForUser(admin).Wait();

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            List<CloudFile> res = service.SearchFileInCurrentDirectory($"@CLOUDROOT\\{admin.Id}", "*").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);

            res = service.SearchFileInCurrentDirectory($"@CLOUDROOT\\{admin.Id}", "#######").GetAwaiter().GetResult();

            Assert.AreEqual(0, res.Count);

            res = service.SearchFileInCurrentDirectory($"@CLOUDROOT\\{admin.Id}", "*est.*").GetAwaiter().GetResult();

            Assert.AreEqual(1, res.Count);
        }

        /// <summary>
        /// Test for user for get current state folders filtered by search pattern errors
        /// </summary>
        [TestMethod]
        public void SearchFileInCurrentStateTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.SearchFileInCurrentDirectory($"@CLOUDROOT\\{admin.Id}\\Test", "*").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to check user storage
        /// </summary>
        [TestMethod]
        public void CheckUserStorageTest()
        {
            admin.UsedSpace = 0;

            context.Update(admin);

            service.CreateBaseDirectoryForUser(admin).Wait();

            string name = "Test.txt";

            File.Copy(name, Path.Combine(Directory.GetCurrentDirectory(), ".__CloudData__", "Private", admin.Id.ToString(), name));

            service.CheckUserStorageUsed(admin).GetAwaiter().GetResult();

            var usr = service.GetAdmin().GetAwaiter().GetResult();

            Assert.IsTrue(usr!.UsedSpace > 0);
        }

        /// <summary>
        /// Test method to get web shared folder by id
        /// </summary>
        [TestMethod]
        public void WebSharedFolderByIdTestSuccess()
        {
            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var folder = context.SharedFolders.First(x => x.Name == "Documents" && x.Owner == admin);

            var res = service.GetWebSharedFolderById(folder.Id).GetAwaiter().GetResult();

            Assert.IsNotNull(res);
            Assert.AreEqual(folder.Name, res.Name);
            Assert.AreEqual(folder.Id, res.Id);
        }

        /// <summary>
        /// Test method for get web shared folder by id errors
        /// </summary>
        [TestMethod]
        public void WebSharedFolderByIdTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetWebSharedFolderById(Guid.NewGuid()).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to get web shared file by id
        /// </summary>
        [TestMethod]
        public void WebSharedFileByIdTestSuccess()
        {
            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Test.txt",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var file = context.SharedFiles.First(x => x.Name == "Test.txt" && x.Owner == admin);

            var res = service.GetWebSharedFileById(file.Id).GetAwaiter().GetResult();

            Assert.IsNotNull(res);
            Assert.AreEqual(file.Name, res.Name);
            Assert.AreEqual(file.Id, res.Id);
        }

        /// <summary>
        /// Test method for get web shared file by id errors
        /// </summary>
        [TestMethod]
        public void WebSharedFileByIdTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetWebSharedFileById(Guid.NewGuid()).GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to get web shared folder by path and name
        /// </summary>
        [TestMethod]
        public void SharedFolderByPatnAndNameTestSuccess()
        {
            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var folder = context.SharedFolders.First(x => x.Name == "Documents" && x.Owner == admin);

            var res = service.GetSharedFolderByPathAndName($"@CLOUDROOT\\{admin.Id}","Documents").GetAwaiter().GetResult();

            Assert.IsNotNull(res);
            Assert.AreEqual(folder.Name, res.Name);
            Assert.AreEqual(folder.Id, res.Id);
        }

        /// <summary>
        /// Test method for get web shared folder by path and name errors
        /// </summary>
        [TestMethod]
        public void SharedFolderByPathAndNameTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetSharedFolderByPathAndName("@CLOUDROOT\\WrongPath","WrongName").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to get web shared file by path and name
        /// </summary>
        [TestMethod]
        public void SharedFileByPathAndNameTestSuccess()
        {
            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}",
                Name = "Test.txt",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var file = context.SharedFiles.First(x => x.Name == "Test.txt" && x.Owner == admin);

            var res = service.GetSharedFileByPathAndName($"@CLOUDROOT\\{admin.Id}","Test.txt").GetAwaiter().GetResult();

            Assert.IsNotNull(res);
            Assert.AreEqual(file.Name, res.Name);
            Assert.AreEqual(file.Id, res.Id);
        }

        /// <summary>
        /// Test method for get web shared file by path and name errors
        /// </summary>
        [TestMethod]
        public void SharedFileByPathAndNameTestErrors()
        {
            Assert.ThrowsException<CloudFunctionStopException>(() => service.GetSharedFileByPathAndName("@CLOUDROOT\\WrongPath","WrongName").GetAwaiter().GetResult());
        }

        /// <summary>
        /// Test method to see if shared path exists in db
        /// </summary>
        [TestMethod]
        public void SharedPathExsitsTest()
        {
            context.Add(new SharedFolder
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}\\Documents",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}\\Documents",
                Name = "Documents",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.Add(new SharedFile
            {
                CloudPathFromRoot = $"@CLOUDROOT\\{admin.Id}\\Music",
                SharedPathFromRoot = $"@SHAREDROOT\\{admin.UserName}\\Music",
                Name = "Test.txt",
                Owner = admin,
                ConnectedToApp = true,
                ConnectedToWeb = true
            });

            context.SaveChanges();

            var res = service.SharedPathExists($"@SHAREDROOT\\{admin.UserName}\\Music").GetAwaiter().GetResult();

            Assert.IsTrue(res);

            res = service.SharedPathExists($"@SHAREDROOT\\{admin.UserName}\\Documents").GetAwaiter().GetResult();

            Assert.IsTrue(res);

            res = service.SharedPathExists($"@SHAREDROOT\\{admin.UserName}\\Test").GetAwaiter().GetResult();

            Assert.IsFalse(res);
        }
    }
}