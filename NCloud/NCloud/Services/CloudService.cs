using NCloud.Models;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using Castle.Core.Internal;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Identity;
using NCloud.ConstantData;
using System.Security.Claims;
using Castle.Core;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;
        private readonly UserManager<CloudUser> userManager;
        //private readonly ILogger logger;

        public CloudService(CloudDbContext context, IWebHostEnvironment env, UserManager<CloudUser> userManager/*, ILogger logger*/)
        {
            this.context = context;
            this.env = env;
            this.userManager = userManager;
            //this.logger = logger;
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

        public async Task CreateDirectory(string folderName, string currentPath, ClaimsPrincipal userPrincipal)
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

                    CloudUser user = await userManager.GetUserAsync(userPrincipal);

                    Pair<string, string> parentPathAndName = GetParentPathAndName(currentPath);

                    Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second, user);

                    await SetDirectoryConnectedState(currentPath, folderName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connections.First, connections.Second);
                }
                else
                {
                    throw new Exception("Folder already exists!");
                }
            }
            catch
            {
                if (!(await RemoveDirectory(folderName, currentPath, userPrincipal)))
                {
                    //TODO: logging action
                }

                throw;
            }
        }

        public async Task<int> CreateFile(IFormFile file, string currentPath, ClaimsPrincipal userPrincipal)
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

                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                Pair<string, string> parentPathAndName = GetParentPathAndName(currentPath);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second, user);

                await SetFileConnectedState(currentPath, newName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connections.First, connections.Second);
            }
            catch
            {
                if (!(await RemoveFile(path, newName, userPrincipal)))
                {
                    //TODO: logging action
                }

                retNum = -1; //error occurred
            }

            return retNum;
        }

        private async Task<Pair<bool, bool>> FolderIsSharedInAppInWeb(string currentPath, string directoryName, CloudUser user)
        {
            SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.SharedPathFromRoot == currentPath && x.Name == directoryName && x.Owner == user);

            if (sharedFolder is null)
            {
                return new Pair<bool, bool>(false, false);
            }

            return new Pair<bool, bool>(sharedFolder.ConnectedToApp, sharedFolder.ConnectedToWeb);
        }

        private Pair<string, string> GetParentPathAndName(string path)
        {
            int index = path.LastIndexOf(Path.DirectorySeparatorChar);

            if (index > -1 && index < path.Length - 1)
            {
                return new Pair<string, string>(path[..index], path[(index + 1)..]);
            }
            else
            {
                return new Pair<string, string>(String.Empty, String.Empty);
            }
        }

        public async Task<CloudUser?> GetAdmin()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.UserName == Constants.AdminUserName);
        }

        public async Task<List<CloudFile>> GetCurrentDepthCloudFiles(string currentPath, ClaimsPrincipal userPrincipal)
        {
            string path = ParseRootName(currentPath);

            CloudUser? user = await userManager.GetUserAsync(userPrincipal);

            var appsharedfiles = user?.SharedFiles.Where(x => x.ConnectedToApp && x.SharedPathFromRoot == currentPath ).Select(x => x.Name).ToList() ?? new();
            var websharedfiles = user?.SharedFiles.Where(x => x.ConnectedToWeb && x.SharedPathFromRoot == currentPath ).Select(x => x.Name).ToList() ?? new();

            try
            {
                return await Task.FromResult<List<CloudFile>>(Directory.GetFiles(path).Select(x => new CloudFile(new FileInfo(Path.Combine(path, x)), appsharedfiles.Contains(Path.GetFileName(x)!), websharedfiles.Contains(Path.GetFileName(x)!))).OrderBy(x => x.Info.Name).ToList());
            }
            catch
            {
                throw new Exception("Error occurred while getting Files!");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string currentPath, ClaimsPrincipal userPrincipal)
        {
            string path = ParseRootName(currentPath);

            CloudUser? user = await userManager.GetUserAsync(userPrincipal);

            var appsharedfolders = user?.SharedFolders.Where(x => x.ConnectedToApp && x.CloudPathFromRoot == currentPath).Select(x => x.Name).ToList() ?? new();
            var websharedfolders = user?.SharedFolders.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == currentPath).Select(x => x.Name).ToList() ?? new();

            try
            {
                return await Task.FromResult<List<CloudFolder>>(Directory.GetDirectories(path).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(path, x)), appsharedfolders.Contains(Path.GetFileName(x)!), websharedfolders.Contains(Path.GetFileName(x)!))).OrderBy(x => x.Info.Name).ToList());
            }
            catch
            {
                throw new Exception("Error occurred while getting Folders!");
            }
        }

        public async Task<bool> RemoveDirectory(string folderName, string currentPath, ClaimsPrincipal userPrincipal)
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

                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                await SetDirectoryConnectedState(currentPath, folderName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, false, false);

            }
            catch (Exception)
            {
                throw new Exception("Could not remove directory");
            }

            return true;
        }

        public async Task<bool> RemoveFile(string fileName, string currentPath, ClaimsPrincipal userPrincipal)
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

                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, false, false);
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
            List<string> pathFolders = path.Split(Path.DirectorySeparatorChar).ToList();
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

        private async Task<bool> SetDirectoryConnectedState(string cloudPath, string directoryName, string sharingPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null)
        {
            try
            {
                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.SharedPathFromRoot == sharingPath && x.Name == directoryName && x.Owner == user);

                if (sharedFolder is null)
                {
                    sharedFolder = new SharedFolder
                    {
                        Name = directoryName,
                        SharedPathFromRoot = sharingPath,
                        CloudPathFromRoot = cloudPath,
                        Owner = user,
                    };

                    if (connectToWeb is not null)
                    {
                        sharedFolder.ConnectedToWeb = connectToWeb.Value;
                    }
                    if (connectToApp is not null)
                    {
                        sharedFolder.ConnectedToApp = connectToApp.Value;
                    }

                    if (sharedFolder.ConnectedToWeb != false || sharedFolder.ConnectedToApp != false)
                    {
                        await context.SharedFolders.AddAsync(sharedFolder);
                    }
                }
                else
                {
                    if (connectToWeb is not null)
                    {
                        sharedFolder.ConnectedToWeb = connectToWeb.Value;
                    }
                    if (connectToApp is not null)
                    {
                        sharedFolder.ConnectedToApp = connectToApp.Value;
                    }

                    if (sharedFolder.ConnectedToWeb == false && sharedFolder.ConnectedToApp == false)
                    {
                        context.SharedFolders.Remove(sharedFolder);
                    }
                    else
                    {
                        context.SharedFolders.Update(sharedFolder);
                    }
                }

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SetObjectAndUnderlyingObjectsState(string currentPath, string directoryName, string dbPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null)
        {
            try
            {
                Queue<Tuple<string, string, DirectoryInfo>> underlyingDirectories = new Queue<Tuple<string, string, DirectoryInfo>>(new List<Tuple<string, string, DirectoryInfo>>() { new Tuple<string, string, DirectoryInfo>(currentPath, dbPath, new DirectoryInfo(Path.Combine(ParseRootName(currentPath), directoryName))) });

                while (underlyingDirectories.Any())
                {
                    var dir = underlyingDirectories.Dequeue();

                    await SetDirectoryConnectedState(dir.Item1, dir.Item3.Name, dir.Item2, user, connectToApp, connectToWeb);

                    string cloudPath = Path.Combine(dir.Item1, dir.Item3.Name);
                    string sharingPath = Path.Combine(dir.Item2, dir.Item3.Name);

                    foreach (string subDirectory in Directory.GetDirectories(dir.Item3.FullName))
                    {
                        DirectoryInfo info = new DirectoryInfo(subDirectory);

                        underlyingDirectories.Enqueue(new Tuple<string, string, DirectoryInfo>(cloudPath, sharingPath, info));

                        await SetDirectoryConnectedState(cloudPath, info.Name, sharingPath, user, connectToApp, connectToWeb);
                    }

                    foreach (string subFile in Directory.GetFiles(dir.Item3.FullName))
                    {
                        await SetFileConnectedState(cloudPath, Path.GetFileName(subFile), sharingPath, user, connectToApp, connectToWeb);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectDirectoryToWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {

            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, currentPath, user, connectToWeb: true);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectDirectoryToApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                Pair<string, string> parentPathAndName = GetParentPathAndName(currentPath);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second, user);

                if (connections.First && currentPath != Constants.GetSharingRootPathInDatabase(user.Id))
                {
                    return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true)
                        && await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true);
                }
                else
                {
                    return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisonnectDirectoryFromApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: false)
                    && await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectDirectoryFromWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, currentPath, user, connectToWeb: false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<bool> SetFileConnectedState(string cloudPath, string fileName, string sharingPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null)
        {
            try
            {
                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.SharedPathFromRoot == sharingPath && x.Name == fileName && x.Owner == user);

                if (sharedFile is null)
                {
                    sharedFile = new SharedFile
                    {
                        Name = fileName,
                        SharedPathFromRoot = sharingPath,
                        CloudPathFromRoot = cloudPath,
                        Owner = user,
                    };

                    if (connectToWeb is not null)
                    {
                        sharedFile.ConnectedToWeb = connectToWeb.Value;
                    }
                    if (connectToApp is not null)
                    {
                        sharedFile.ConnectedToApp = connectToApp.Value;
                    }

                    if (sharedFile.ConnectedToWeb != false || sharedFile.ConnectedToApp != false)
                    {
                        await context.SharedFiles.AddAsync(sharedFile);
                    }
                }
                else
                {
                    if (connectToWeb is not null)
                    {
                        sharedFile.ConnectedToWeb = connectToWeb.Value;
                    }
                    if (connectToApp is not null)
                    {
                        sharedFile.ConnectedToApp = connectToApp.Value;
                    }

                    if (sharedFile.ConnectedToWeb != false && sharedFile.ConnectedToApp != false)
                    {
                        context.SharedFiles.Remove(sharedFile);
                    }
                    else
                    {
                        context.SharedFiles.Update(sharedFile);
                    }
                }

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectFileToApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                Pair<string, string> parentPathAndName = GetParentPathAndName(currentPath);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second, user);

                if (connections.First && currentPath != Constants.GetSharingRootPathInDatabase(user.Id))
                {
                    return await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true)
                        && await SetFileConnectedState(Constants.GetSharingRootPathInDatabase(user.Id), fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true);
                }
                else
                {
                    return await SetFileConnectedState(Constants.GetSharingRootPathInDatabase(user.Id), fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true);
                }
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

                return await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToWeb: true);
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

                return await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: false)
                    && await SetFileConnectedState(Constants.GetSharingRootPathInDatabase(user.Id), fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: false);
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

                return await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToWeb: false);
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

            return (await context.SharedFiles.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == currentPath).ToListAsync()).Select(x => new CloudFile(x.Name)).ToList() ?? new();
        }

        public async Task<List<CloudFolder>> GetCurrentDepthSharingDirectories(string currentPath, ClaimsPrincipal userPrincipal)
        {
            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(currentPath));

            return (await context.SharedFolders.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == currentPath).ToListAsync()).Select(x => new CloudFolder(x.Name)).ToList() ?? new();
        }

        private string ChangeOwnerIdentification(string path, string? itemForChange)
        {
            if (itemForChange is null)
                return path;

            try
            {
                var data = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                data[Constants.OwnerPlaceInPath] = itemForChange;
                return Path.Combine(data);
            }
            catch
            {
                return path;
            }
        }

        private string GetSharedPathOwnerUser(string currentPath)
        {
            try
            {
                return currentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            catch (IndexOutOfRangeException)
            {
                return String.Empty;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> OwnerOfPathIsActualUser(string sharingCurrentPath, ClaimsPrincipal userPrincipal)
        {
            CloudUser? user = await userManager.GetUserAsync(userPrincipal);

            if (user is null)
                return false;

            try
            {
                return GetSharedPathOwnerUser(sharingCurrentPath) == user.UserName;
            }
            catch
            {
                return false;
            }
        }
    }
}
