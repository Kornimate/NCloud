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
using NCloud.Security;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System;
using System.Xml.Linq;
using Microsoft.VisualBasic.FileIO;

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

                    Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                    await SetDirectoryConnectedState(currentPath, folderName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connections.First, connections.Second);
                }
                else
                {
                    throw new InvalidOperationException("Folder already exists!");
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                if (!(await RemoveDirectory(folderName, currentPath, userPrincipal)))
                {
                    //TODO: logging action
                }

                throw;
            }
        }

        public async Task<string> CreateFile(IFormFile file, string currentPath, ClaimsPrincipal userPrincipal)
        {
            string path = ParseRootName(currentPath);
            string newName = file.FileName;

            try
            {
                string pathAndName = Path.Combine(path, RenameObject(path, ref newName, true));

                using (FileStream stream = new FileStream(pathAndName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                Pair<string, string> parentPathAndName = GetParentPathAndName(currentPath);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                await SetFileConnectedState(currentPath, newName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connections.First, connections.Second);
            }
            catch
            {
                if (!(await RemoveFile(path, newName, userPrincipal)))
                {
                    //TODO: logging action
                }

                return String.Empty;
            }

            return newName;
        }

        private async Task<Pair<bool, bool>> FolderIsSharedInAppInWeb(string cloudPath, string directoryName)
        {
            SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == directoryName);

            if (sharedFolder is null)
            {
                return new Pair<bool, bool>(false, false);
            }

            return new Pair<bool, bool>(sharedFolder.ConnectedToApp, sharedFolder.ConnectedToWeb);
        }

        private async Task<Pair<bool, bool>> FileIsSharedInAppInWeb(string cloudPath, string directoryName)
        {
            SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == directoryName);

            if (sharedFile is null)
            {
                return new Pair<bool, bool>(false, false);
            }

            return new Pair<bool, bool>(sharedFile.ConnectedToApp, sharedFile.ConnectedToWeb);
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

        public async Task<List<CloudFile>> GetCurrentDepthCloudFiles(string currentPath, bool connectedToApp = false, bool connectedToWeb = false)
        {
            string path = ParseRootName(currentPath);

            var appsharedfiles = context.SharedFiles.Where(x => x.ConnectedToApp && x.CloudPathFromRoot == currentPath).Select(x => x.Name).ToList() ?? new();
            var websharedfiles = context.SharedFiles.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == currentPath).Select(x => x.Name).ToList() ?? new();

            try
            {
                var items = await Task.FromResult<IEnumerable<CloudFile>>(Directory.GetFiles(path).Select(x => new CloudFile(new FileInfo(x), appsharedfiles.Contains(Path.GetFileName(x)!), websharedfiles.Contains(Path.GetFileName(x)!), Path.Combine(currentPath, Path.GetFileName(x)))).OrderBy(x => x.Info.Name));

                if (connectedToApp)
                {
                    items = items.Where(x => x.IsConnectedToApp);
                }
                else if (connectedToWeb)
                {
                    items = items.Where(x => x.IsConnectedToWeb);
                }

                return items.ToList();
            }
            catch
            {
                throw new Exception("Error occurred while getting Files!");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string currentPath, bool connectedToApp = false, bool connectedToWeb = false)
        {
            string path = ParseRootName(currentPath);

            var appsharedfolders = await context.SharedFolders.Where(x => x.ConnectedToApp && x.CloudPathFromRoot == currentPath).Select(x => x.Name).ToListAsync() ?? new();
            var websharedfolders = await context.SharedFolders.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == currentPath).Select(x => x.Name).ToListAsync() ?? new();

            try
            {
                var items = await Task.FromResult<IEnumerable<CloudFolder>>(Directory.GetDirectories(path).Select(x => new CloudFolder(new DirectoryInfo(x), appsharedfolders.Contains(Path.GetFileName(x)!), websharedfolders.Contains(Path.GetFileName(x)!), Path.Combine(currentPath, Path.GetFileName(x)))).OrderBy(x => x.Info.Name));

                if (connectedToApp)
                {
                    items = items.Where(x => x.IsConnectedToApp);
                }
                else if (connectedToWeb)
                {
                    items = items.Where(x => x.IsConnectedToWeb);
                }

                return items.ToList();
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

                await SetObjectAndUnderlyingObjectsState(currentPath, folderName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, false, false);

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
            else if (currentPath.StartsWith(Constants.PublicRootName))
            {
                return currentPath.Replace(Constants.PublicRootName, Path.Combine(env.WebRootPath, "CloudData", "Public"));
            }

            return String.Empty;
        }

        public string ChangeRootName(string currentPath)
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

            if (pathFolders.Count < 1)
                return false;

            return Constants.SystemFolders.Contains(pathFolders.Last()) && ((pathFolders.Count - pathFolders.FindIndex(x => x == Constants.WebRootFolderName) - 1) == Constants.DistanceToRootFolder);
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
                if (!SecurityManager.CheckIfDirectoryExists(Path.Combine(ParseRootName(cloudPath), directoryName)))
                {
                    throw new FileNotFoundException("File does not exists!");
                }

                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == directoryName && x.Owner == user);

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
                        sharedFolder.SharedPathFromRoot = sharingPath;

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
                        sharedFolder.SharedPathFromRoot = sharingPath;
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

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                if (connections.First && currentPath != Constants.GetCloudRootPathInDatabase(user.Id))
                {
                    return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true)
                        && await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToApp: true);
                }
                else
                {
                    return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToApp: true);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectDirectoryFromApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                return await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: false)
                    && await SetObjectAndUnderlyingObjectsState(currentPath, directoryName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToApp: false);
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
                if (!SecurityManager.CheckIfFileExists(Path.Combine(ParseRootName(cloudPath), fileName)))
                {
                    throw new FileNotFoundException("File does not exists!");
                }

                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == fileName && x.Owner == user);

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
                        sharedFile.SharedPathFromRoot = sharingPath;
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
                        sharedFile.SharedPathFromRoot = sharingPath;
                    }

                    if (sharedFile.ConnectedToWeb == false && sharedFile.ConnectedToApp == false)
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

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                if (connections.First && currentPath != Constants.GetCloudRootPathInDatabase(user.Id))
                {
                    return await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: true)
                        && await SetFileConnectedState(currentPath, fileName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToApp: true);
                }
                else
                {
                    return await SetFileConnectedState(currentPath, fileName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToApp: true);
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

                return await SetFileConnectedState(currentPath, fileName, currentPath, user, connectToWeb: true);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectFileFromApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                return await SetFileConnectedState(currentPath, fileName, ChangeOwnerIdentification(ChangeRootName(currentPath), user.UserName), user, connectToApp: false)
                    && await SetFileConnectedState(currentPath, fileName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToApp: false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectFileFromWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal)
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                return await SetFileConnectedState(currentPath, fileName, currentPath, user, connectToWeb: false);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<List<CloudFolder>> GetSharingUsersSharingDirectories(string currentPath)
        {
            return context.Users.Where(x => x.SharedFiles.Where(x => x.ConnectedToApp).Count() > 0 || x.SharedFolders.Where(x => x.ConnectedToApp).Count() > 0).Select(x => new CloudFolder(x.UserName, null)).ToListAsync();
        }

        public async Task<List<CloudFile>> GetCurrentDepthAppSharingFiles(string currentPath)
        {
            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(currentPath));

            return (await context.SharedFiles.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == currentPath).ToListAsync()).Select(x => new CloudFile(new FileInfo(Path.Combine(ParseRootName(x.CloudPathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb, String.Empty)).ToList() ?? new();
        }

        public async Task<List<CloudFolder>> GetCurrentDepthAppSharingDirectories(string currentPath)
        {
            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(currentPath));

            return (await context.SharedFolders.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == currentPath).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(ParseRootName(x.CloudPathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb, String.Empty)).ToList() ?? new();
        }

        private static string ChangeOwnerIdentification(string path, string? itemForChange)
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

        private static string GetSharedPathOwnerUser(string currentPath)
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

        public async Task<List<string>> GetUserSharedFolderUrls(ClaimsPrincipal userPrincipal)
        {
            CloudUser? user = await userManager.GetUserAsync(userPrincipal);

            return await context.SharedFolders.Where(x => x.Owner == user && x.ConnectedToWeb).OrderBy(x => x.CloudPathFromRoot).ThenBy(x => x.Name).Select(x => Path.Combine(x.CloudPathFromRoot, x.Name)).ToListAsync();
        }

        public async Task<List<string>> GetUserSharedFileUrls(ClaimsPrincipal userPrincipal)
        {
            CloudUser? user = await userManager.GetUserAsync(userPrincipal);

            return await context.SharedFiles.Where(x => x.Owner == user && x.ConnectedToWeb).OrderBy(x => x.CloudPathFromRoot).ThenBy(x => x.Name).Select(x => Path.Combine(x.CloudPathFromRoot, x.Name)).ToListAsync();
        }

        public async Task<string> WebBackCheck(string path)
        {
            Pair<string, string> parentPathAndName = GetParentPathAndName(path);

            if (await FolderIsSharedInWeb(parentPathAndName.First, parentPathAndName.Second))
            {
                return parentPathAndName.First;
            }

            return path;
        }

        private async Task<bool> FolderIsSharedInWeb(string path, string folderName)
        {
            return await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == path && x.Name == folderName && x.ConnectedToWeb) != null;
        }

        public async Task<List<CloudFile>> GetCurrentDepthWebSharingFiles(string path)
        {
            try
            {
                return (await context.SharedFiles.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == path).ToListAsync()).Select(x => new CloudFile(new FileInfo(ParseRootName(Path.Combine(x.CloudPathFromRoot, x.Name))), false, true, String.Empty)).OrderBy(x => x.Info.Name).ToList();
            }
            catch
            {
                throw new Exception("Error occurred while getting Files!");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthWebSharingDirectories(string path)
        {
            try
            {
                return (await context.SharedFolders.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == path).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(ParseRootName(Path.Combine(x.CloudPathFromRoot, x.Name))), false, true, String.Empty)).OrderBy(x => x.Info.Name).ToList();
            }
            catch
            {
                throw new Exception("Error occurred while getting Folders!");
            }
        }

        public async Task<string?> CreateZipFile(List<string> itemsForDownload, string currentPath, string filePath, bool connectedToApp, bool connectedToWeb)
        {
            using var zipFile = System.IO.File.Create(filePath);

            try
            {
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    foreach (string itemName in itemsForDownload!)
                    {
                        if (itemName != Constants.NotSelectedResult && itemName is not null && itemName != String.Empty)
                        {
                            if (itemName.StartsWith(Constants.SelectedFileStarterSymbol))
                            {
                                try
                                {
                                    string name = itemName[1..];

                                    await AddFileToArchive(archive, currentPath, name, connectedToApp, connectedToWeb);
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"{ex.Message} ({itemName})");
                                }
                            }
                            else if (itemName.StartsWith(Constants.SelectedFolderStarterSymbol))
                            {
                                try
                                {
                                    string name = itemName[1..];
                                    string serverPathStart = ParseRootName(currentPath);
                                    string currentRelativePath = String.Empty;

                                    Queue<Pair<string, DirectoryInfo>> directories = new(new List<Pair<string, DirectoryInfo>>() { new Pair<string, DirectoryInfo>(currentRelativePath, await GetFolderByPath(serverPathStart, name)) });

                                    while (directories.Any())
                                    {
                                        var directoryInfo = directories.Dequeue();

                                        int counter = 0;

                                        currentRelativePath = Path.Combine(directoryInfo.First, directoryInfo.Second.Name);

                                        foreach (CloudFile file in await GetCurrentDepthCloudFiles(Path.Combine(currentPath, currentRelativePath), connectedToApp, connectedToWeb))
                                        {
                                            archive.CreateEntryFromFile(Path.Combine(serverPathStart, currentRelativePath, file.Info.Name), Path.Combine(currentRelativePath, file.Info.Name));

                                            ++counter;
                                        }

                                        foreach (CloudFolder folder in await GetCurrentDepthCloudDirectories(Path.Combine(currentPath, currentRelativePath), connectedToApp, connectedToWeb))
                                        {
                                            directories.Enqueue(new Pair<string, DirectoryInfo>(currentRelativePath, folder.Info));

                                            ++counter;
                                        }

                                        if (counter == 0)
                                        {
                                            archive.CreateEntry(currentRelativePath).ExternalAttributes = Constants.EmptyFolderAttributeNumberZip;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"{ex.Message} ({itemName})");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"The following item is invalid: {itemName}, will not be in the created file");
                        }
                    }
                }

                return filePath;
            }
            catch (Exception ex)
            {
                zipFile.Close();

                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception) { }
                }

                throw new InvalidOperationException(ex.Message);

            }
        }

        private async Task AddFileToArchive(ZipArchive archive, string currentPath, string name, bool connectedToApp = false, bool connectedToWeb = false)
        {
            if (connectedToApp)
            {
                var connections = await FileIsSharedInAppInWeb(currentPath, name);

                if (connections.First)
                {
                    archive.CreateEntryFromFile(Path.Combine(ParseRootName(currentPath), name), name);
                }
            }
            else if (connectedToWeb)
            {
                var connections = await FileIsSharedInAppInWeb(currentPath, name);

                if (connections.Second)
                {
                    archive.CreateEntryFromFile(Path.Combine(ParseRootName(currentPath), name), name);
                }
            }
            else
            {
                archive.CreateEntryFromFile(Path.Combine(ParseRootName(currentPath), name), name);
            }
        }

        public async Task<string> ChangePathStructure(string currentPath)
        {
            string[] pathElements = currentPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            if (pathElements.Length < 2)
            {
                return currentPath;
            }

            if (pathElements[Constants.RootProviderPlaceinPath] == Constants.PublicRootName)
            {
                CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == pathElements[Constants.OwnerPlaceInPath]);

                if (user is not null)
                {
                    pathElements[Constants.RootProviderPlaceinPath] = Constants.PrivateRootName;
                    pathElements[Constants.OwnerPlaceInPath] = user.Id.ToString();

                    return String.Join(Path.DirectorySeparatorChar, pathElements);
                }
                else
                {
                    return currentPath;
                }
            }

            if (pathElements[Constants.RootProviderPlaceinPath] == Constants.PrivateRootName)
            {
                CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.Id.ToString() == pathElements[Constants.OwnerPlaceInPath]);

                if (user is not null)
                {
                    pathElements[Constants.RootProviderPlaceinPath] = Constants.PublicRootName;
                    pathElements[Constants.OwnerPlaceInPath] = user.UserName;

                    return String.Join(Path.DirectorySeparatorChar, pathElements);
                }
                else
                {
                    return currentPath;
                }
            }

            return currentPath;
        }

        public Task<bool> ModifyFileContent(string file, string content)
        {
            try
            {
                File.WriteAllText(ParseRootName(file), content);

                return Task.FromResult<bool>(true);
            }
            catch (Exception)
            {
                return Task.FromResult<bool>(false);
            }
        }

        public async Task<CloudFolder> GetFolder(string currentPath, string folderName)
        {
            var folder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == currentPath && x.Name == folderName);

            return await Task.FromResult<CloudFolder>(new CloudFolder(new DirectoryInfo(Path.Combine(ParseRootName(currentPath), folderName)), folder?.ConnectedToApp ?? false, folder?.ConnectedToWeb ?? false, Path.Combine(currentPath, folderName)));
        }

        public async Task<CloudFile> GetFile(string currentPath, string fileName)
        {
            var file = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == currentPath && x.Name == fileName);

            return await Task.FromResult<CloudFile>(new CloudFile(new FileInfo(Path.Combine(ParseRootName(currentPath), fileName)), file?.ConnectedToApp ?? false, file?.ConnectedToWeb ?? false, Path.Combine(currentPath, fileName)));
        }

        public async Task<string> RenameFolder(string currentPath, string folderName, string newName)
        {
            try
            {
                string folderPath = ParseRootName(currentPath);
                string folderPathAndName = Path.Combine(folderPath, folderName);

                if (IsSystemFolder(folderPathAndName))
                    throw new Exception("System Folders can not be renamed!");

                string newFolderPathAndName = Path.Combine(folderPath, newName);

                if (Directory.Exists(newFolderPathAndName))
                    throw new Exception("Folder with this name already exists");

                Directory.Move(folderPathAndName, newFolderPathAndName);

                SharedFolder? folder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == currentPath && x.Name == folderName);

                if (folder is not null)
                {
                    folder.Name = newName;

                    context.SharedFolders.Update(folder);
                    await context.SaveChangesAsync();
                }

                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> RenameFile(string currentPath, string fileName, string newName)
        {
            string newFileName = new string(newName);

            try
            {
                string filePath = ParseRootName(currentPath);

                File.Move(Path.Combine(filePath, fileName), Path.Combine(filePath, RenameObject(filePath, ref newFileName, true)));

                SharedFile? file = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == currentPath && x.Name == fileName);

                if (file is not null)
                {
                    file.Name = newFileName;

                    context.SharedFiles.Update(file);
                    await context.SaveChangesAsync();
                }

                return fileName;
            }
            catch (Exception)
            {
                throw new FileLoadException("Error while renaming file");
            }
        }

        private string RenameObject(string path, ref string name, bool isFile)
        {
            int counter = 0;

            string pathAndName = Path.Combine(path, name);

            if (isFile)
            {
                string nameBase = new string(name);
                string extension = String.Empty;

                if (name.Contains(Constants.FileExtensionDelimiter))
                {
                    nameBase = Path.GetFileNameWithoutExtension(name);
                    extension = Path.GetExtension(name);
                }

                while (System.IO.File.Exists(pathAndName))
                {
                    name = nameBase + Constants.FileNameDelimiter + (++counter).ToString() + extension;

                    pathAndName = Path.Combine(path, name);
                }
            }
            else
            {
                string nameBase = new string(name);

                while (System.IO.Directory.Exists(pathAndName))
                {
                    name = nameBase + Constants.FileNameDelimiter + (++counter).ToString();

                    pathAndName = Path.Combine(path, name);
                }
            }

            return name;
        }

        public async Task<string> CopyFile(string? source, string destination)
        {
            if (source is null || source == String.Empty)
            {
                throw new Exception("Invalid source of file");
            }

            if (destination is null || destination == String.Empty)
            {
                throw new Exception("Invalid destination for copy");
            }

            try
            {
                string src = ParseRootName(source);
                string dest = ParseRootName(destination);

                FileInfo fi = new FileInfo(src);

                string name = new string(fi.Name);

                File.Copy(src,Path.Combine(dest, RenameObject(dest, ref name, true)));

                if (name == fi.Name)
                {
                    return await Task.FromResult<string>(String.Empty);
                }

                return await Task.FromResult<string>(name);
            }
            catch (Exception)
            {
                throw new Exception("Error while pasting file");
            }
        }

        public async Task<string> CopyFolder(string? source, string destination)
        {

            throw new Exception();
        }
    }
}
