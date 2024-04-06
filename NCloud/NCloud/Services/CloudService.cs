using NCloud.Models;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using Castle.Core.Internal;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using static NuGet.Packaging.PackagingConstants;
using NCloud.ConstantData;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;

        public CloudService(CloudDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;

        }
        public async Task<bool> CreateBaseDirectory(CloudUser cloudUser)
        {
            string privateFolderPath = Path.Combine(env.WebRootPath, "CloudData", "Private", cloudUser.Id.ToString());
            string publicFolderPath = Path.Combine(env.WebRootPath, "CloudData", "Public");
            string pathHelper = Path.Combine(env.WebRootPath, "CloudData", "Public", cloudUser.UserName);

            try
            {
                if (!Directory.Exists(privateFolderPath))
                {
                    Directory.CreateDirectory(privateFolderPath);
                }

                if (!Directory.Exists(publicFolderPath))
                {
                    Directory.CreateDirectory(publicFolderPath);
                }

                if (!Directory.Exists(pathHelper))
                {
                    Directory.CreateDirectory(pathHelper);
                }

                foreach (string folder in Constants.SystemFolders)
                {
                    pathHelper = Path.Combine(privateFolderPath, folder);

                    if (!Directory.Exists(pathHelper))
                    {
                        Directory.CreateDirectory(pathHelper);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                if (!await DeleteDirectoriesForUser(publicFolderPath, privateFolderPath, pathHelper))
                {
                    //TODO: logging for failed clean up
                }

                return false; //if anything can not be created -> fail the whole task
            }
        }

        private Task<bool> DeleteDirectoriesForUser(string publicFolderPath, string privateFolderPath, string pathHelper)
        {
            try
            {
                if (!Directory.Exists(privateFolderPath))
                {
                    Directory.Delete(privateFolderPath, true);
                }

                if (!Directory.Exists(publicFolderPath))
                {
                    Directory.Delete(publicFolderPath, true);
                }

                if (!Directory.Exists(pathHelper))
                {
                    Directory.Delete(pathHelper);
                }

                foreach (string folder in Constants.SystemFolders)
                {
                    pathHelper = Path.Combine(privateFolderPath, folder);

                    if (!Directory.Exists(pathHelper))
                    {
                        Directory.Delete(pathHelper, true);
                    }
                }

                return Task.FromResult<bool>(true);
            }
            catch (Exception)
            {
                return Task.FromResult<bool>(false);
            }
        }

        public async Task CreateDirectory(string folderName, string currentPath)
        {
            if (folderName == null || folderName == String.Empty)
            {
                throw new Exception("Invalid Folder Name!");
            }

            if (currentPath == null || currentPath == String.Empty)
            {
                throw new Exception("Invalid Path!");
            }

            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, folderName);

            try
            {
                if (!DirectoryExists(pathAndName))
                {
                    await Task.Run(() => Directory.CreateDirectory(pathAndName));
                }
                else
                {
                    throw new Exception("Folder already exists!");
                }
            }
            catch
            {
                if (!(await RemoveDirectory(folderName, currentPath)))
                {
                    //TODO: logging action
                }

                throw;
            }
        }

        public async Task<int> CreateFile(IFormFile file, string currentPath)
        {
            int retNum = 1;
            
            string path = ParseRootName(currentPath);
            string newName = file.FileName;
            string pathAndName = Path.Combine(path, newName);
            
            try
            {
                int counter = 0;

                while (System.IO.File.Exists(pathAndName))
                {
                    FileInfo fi = new FileInfo(file.FileName);
                    newName = fi.Name.Split('.')[0] + Constants.FileNameDelimiter + $"{++counter}" + fi.Extension;
                    pathAndName = Path.Combine(path, newName);
                    retNum = 0;
                }

                using (FileStream stream = new FileStream(pathAndName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch
            {
                if (!(await RemoveFile(path, newName)))
                {
                    //TODO: logging action
                }

                retNum = -1; //error occurred
            }

            return retNum;
        }

        public async Task<CloudUser?> GetAdmin()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.UserName == Constants.AdminUserName);
        }

        public async Task<List<CloudFile>> GetCurrentDepthFiles(string currentPath)
        {
            string path = ParseRootName(currentPath);

            try
            {
                return await Task.FromResult<List<CloudFile>>(Directory.GetFiles(path).Select(x => new CloudFile(new FileInfo(Path.Combine(path, x)), false, false)).ToList());
            }
            catch
            {
                throw new Exception("Error occurred while getting Files!");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthDirectories(string currentPath)
        {
            string path = ParseRootName(currentPath);
            try
            {
                return await Task.FromResult<List<CloudFolder>>(Directory.GetDirectories(path).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(path, x)), false, false)).ToList());
            }
            catch
            {
                throw new Exception("Error occurred while getting Folders!");
            }
        }

        public async Task<bool> RemoveDirectory(string folderName, string currentPath)
        {
            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, folderName);

            if (!Directory.Exists(pathAndName))
            {
                throw new Exception("The Folder does not exists!");
            }

            if (IsSystemFolder(pathAndName))
            {
                return false;
            }

            if (path is null || path == String.Empty)
            {
                throw new ArgumentException("Invalid path!");
            }

            if (folderName is null || folderName == String.Empty)
            {
                throw new ArgumentException("Invalid Folder Name!");
            }

            await Task.Run(() => Directory.Delete(pathAndName, true));

            return true;
        }

        public async Task<bool> RemoveFile(string fileName, string currentPath)
        {
            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, fileName);
            if (!File.Exists(pathAndName))
            {
                throw new Exception("The File does not exists!");
            }
            if (path is null || path == String.Empty)
            {
                throw new ArgumentException("Invalid path!");
            }
            if (fileName is null || fileName == String.Empty)
            {
                throw new ArgumentException("Invalid Folder Name!");
            }
            try
            {
                await Task.Run(() => File.Delete(pathAndName));
            }
            catch
            {
                throw new Exception("Could not remove file");
            }
            return true;
        }

        private string ParseRootName(string currentPath)
        {
            if (currentPath.StartsWith(Constants.PrivateRootName))
            {
                return currentPath.Replace(Constants.PrivateRootName, Path.Combine(env.WebRootPath, "CloudData", "Private"));
            }
            else
            {
                return currentPath.Replace(Constants.PublicRootName, Path.Combine(env.WebRootPath, "CloudData", "Public"));
            }
        }

        private bool IsSystemFolder(string path)
        {
            List<string> pathFolders = path.Split('\\').ToList();
            return Constants.SystemFolders.Contains(pathFolders[pathFolders.FindIndex(x => x == "wwwroot") + Constants.DistanceToRootFolder]);
        }

        public string ServerPath(string currentPath)
        {
            return ParseRootName(currentPath);
        }

        public Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData()
        {
            return new Tuple<List<CloudFile?>, List<CloudFolder?>>(new(), new());
        }

        public bool DirectoryExists(string? pathAndName)
        {
            if (pathAndName is null) return false;
            return Directory.Exists(ParseRootName(pathAndName));
        }

        public async Task<DirectoryInfo> GetFolderByPath(string serverPath, string folderName)
        {
            return await Task.FromResult<DirectoryInfo>(new DirectoryInfo(Directory.GetDirectories(serverPath,folderName).First()));
        }
    }
}
