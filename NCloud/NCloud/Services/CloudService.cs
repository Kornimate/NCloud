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
using System.Security.Claims;
using Microsoft.Build.Evaluation;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;
        private readonly UserManager<CloudUser> userManager;

        public CloudService(CloudDbContext context, IWebHostEnvironment env, UserManager<CloudUser> userManager)
        {
            this.context = context;
            this.env = env;
            this.userManager = userManager;

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

        public async Task<List<CloudFile>> GetCurrentDepthCloudFiles(string currentPath)
        {
            string path = ParseRootName(currentPath);

            try
            {
                return await Task.FromResult<List<CloudFile>>(Directory.GetFiles(path).Select(x => new CloudFile(new FileInfo(Path.Combine(path, x)), false, false)).OrderBy(x => x.Info.Name).ToList());
            }
            catch
            {
                throw new Exception("Error occurred while getting Files!");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string currentPath)
        {
            string path = ParseRootName(currentPath);

            CloudUser? user = await GetAdmin();

            var websharedfolders = user?.SharedFolders.Where(x => x.ConnectedToWeb && x.PathFromRoot == currentPath).Select(x => x.Name).ToList() ?? new();
            var appsharedfolders = user?.SharedFolders.Where(x => x.ConnectedToApp && x.PathFromRoot == currentPath).Select(x => x.Name).ToList() ?? new();

            try
            {
                return await Task.FromResult<List<CloudFolder>>(Directory.GetDirectories(path).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(path, x)), appsharedfolders.Contains(Path.GetFileName(x)!), websharedfolders.Contains(Path.GetFileName(x)!))).OrderBy(x => x.Info.Name).ToList());
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

            try
            {
                await Task.Run(() => Directory.Delete(pathAndName, true));
            }
            catch (Exception)
            {
                throw new Exception("Could not remove directory");
            }

            //TODO: implement to remove it from db if exists

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

            //TODO: implement to remove it from db if exists

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

        private string ChangeRootName(string currentPath)
        {
            if (currentPath.StartsWith(Constants.PrivateRootName))
            {
                return currentPath.Replace(Constants.PrivateRootName, Constants.PublicRootName);
            }
            else if (currentPath.StartsWith(Constants.PublicRootName))
            {
                return currentPath.Replace(Constants.PublicRootName, Constants.PrivateRootName);
            }

            return currentPath;
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
            return await Task.FromResult<DirectoryInfo>(new DirectoryInfo(Directory.GetDirectories(serverPath, folderName).First()));
        }

        public async Task<int> ConnectDirectoryToWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {

            //TODO: implement itreative function to add underlying objects
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == directoryName && x.Owner == user);

                if (sharedFolder is null)
                {
                    context.SharedFolders.Add(new SharedFolder
                    {
                        Name = directoryName,
                        PathFromRoot = currentPath,
                        Owner = user,
                        ConnectedToWeb = true,
                        ConnectedToApp = false
                    });
                }
                else
                {
                    sharedFolder.ConnectedToWeb = true;

                    context.SharedFolders.Update(sharedFolder);
                }

                await context.SaveChangesAsync();

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> ConnectDirectoryToApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == directoryName && x.Owner == user);

                if (sharedFolder is null)
                {
                    context.SharedFolders.Add(new SharedFolder
                    {
                        Name = directoryName,
                        PathFromRoot = currentPath,
                        Owner = user,
                        ConnectedToWeb = false,
                        ConnectedToApp = true
                    });
                }
                else
                {
                    sharedFolder.ConnectedToApp = true;

                    context.SharedFolders.Update(sharedFolder);
                }

                await context.SaveChangesAsync();

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> DisonnectDirectoryFromApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == directoryName && x.Owner == user);

                if (sharedFolder is null)
                {
                    return 0;
                }

                if (sharedFolder.ConnectedToWeb)
                {
                    sharedFolder.ConnectedToApp = false;

                    await context.SaveChangesAsync();

                    return 0;
                }

                context.SharedFolders.Remove(sharedFolder);

                await context.SaveChangesAsync();

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<int> DisconnectDirectoryFromWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == directoryName && x.Owner == user);

                if (sharedFolder is null)
                {
                    return 0;
                }

                if (sharedFolder.ConnectedToApp)
                {
                    sharedFolder.ConnectedToWeb = false;

                    await context.SaveChangesAsync();

                    return 0;
                }

                context.SharedFolders.Remove(sharedFolder);

                await context.SaveChangesAsync();

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> ConnectFileToApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == fileName && x.Owner == user);

                if (sharedFile is null)
                {
                    context.SharedFiles.Add(new SharedFile
                    {
                        Name = fileName,
                        PathFromRoot = currentPath,
                        Owner = user,
                        ConnectedToWeb = false,
                        ConnectedToApp = true
                    });
                }
                else
                {
                    sharedFile.ConnectedToApp = true;

                    context.SharedFiles.Update(sharedFile);
                }

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectFileToWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == fileName && x.Owner == user);

                if (sharedFile is null)
                {
                    context.SharedFiles.Add(new SharedFile
                    {
                        Name = fileName,
                        PathFromRoot = currentPath,
                        Owner = user,
                        ConnectedToWeb = true,
                        ConnectedToApp = false
                    });
                }
                else
                {
                    sharedFile.ConnectedToWeb = true;

                    context.SharedFiles.Update(sharedFile);
                }

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisonnectFileFromApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == fileName && x.Owner == user);

                if (sharedFile is null)
                {
                    return false;
                }

                if (sharedFile.ConnectedToWeb)
                {
                    sharedFile.ConnectedToApp = false;

                    await context.SaveChangesAsync();

                    return true;
                }

                context.SharedFiles.Remove(sharedFile);

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisonnectFileFromWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.PathFromRoot == currentPath && x.Name == fileName && x.Owner == user);

                if (sharedFile is null)
                {
                    return false;
                }

                if (sharedFile.ConnectedToApp)
                {
                    sharedFile.ConnectedToWeb = false;

                    await context.SaveChangesAsync();

                    return true;
                }

                context.SharedFiles.Remove(sharedFile);

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<List<CloudFolder>> GetSharingUsersSharingDirectories(string currentPath)
        {
            return context.Users.Where(x => x.SharedFiles.Count > 0 || x.SharedFolders.Count > 0).Select(x => new CloudFolder(x.UserName, null)).ToListAsync();
        }

        public async Task<List<CloudFile>> GetCurrentDepthSharingFiles(string currentPath, ClaimsPrincipal userPrincipal)
        {
            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(currentPath));

            string path = ChangeUserNameToId(ChangeRootName(currentPath), user?.Id.ToString(), user?.UserName);

            return (await context.SharedFiles.Where(x => x.ConnectedToApp && x.Owner == user && x.PathFromRoot == path).ToListAsync()).Select(x => new CloudFile(new FileInfo(ParseRootName(x.PathFromRoot)), x.ConnectedToApp, x.ConnectedToWeb)).ToList() ?? new();
        }

        public async Task<List<CloudFolder>> GetCurrentDepthSharingDirectories(string currentPath, ClaimsPrincipal userPrincipal)
        {
            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(currentPath));

            string path = ChangeUserNameToId(ChangeRootName(currentPath), user?.Id.ToString(), user?.UserName);

            return (await context.SharedFolders.Where(x => x.ConnectedToApp && x.Owner == user && x.PathFromRoot == path).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(ParseRootName(x.PathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb)).ToList() ?? new();
        }

        private string ChangeUserNameToId(string path, string? id, string? userName)
        {
            if (id is null || userName is null)
                return path;

            return path.Replace(userName, id);
        }

        private string GetSharedPathOwnerUser(string currentPath)
        {
            return currentPath.Contains(Path.DirectorySeparatorChar) ? currentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)[1] : String.Empty;
        }
    }
}
