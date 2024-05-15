using NCloud.Models;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using NCloud.ConstantData;
using System.Security.Claims;
using Castle.Core;
using NCloud.Services.Exceptions;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using NCloud.Security;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;
        private readonly IHttpContextAccessor httpContext;
        private readonly UserManager<CloudUser> userManager;

        public CloudService(CloudDbContext context, IHttpContextAccessor httpContext, UserManager<CloudUser> userManager)
        {
            this.context = context;
            this.httpContext = httpContext;
            this.userManager = userManager;
        }

        #region Public Methods 
        public async Task<CloudUser?> GetAdmin()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.UserName == Constants.AdminUserName);
        }

        public async Task<bool> CreateBaseDirectoryForUser(CloudUser cloudUser)
        {
            string privateFolderPath = Constants.GetPrivateBaseDirectoryForUser(cloudUser.Id.ToString());

            try
            {
                if (!Directory.Exists(privateFolderPath)) //creating user folder
                {
                    Directory.CreateDirectory(privateFolderPath);
                }

                foreach (string folder in Constants.SystemFolders) //creating system folders
                {
                    string pathHelper = Path.Combine(privateFolderPath, folder);

                    if (!Directory.Exists(pathHelper))
                    {
                        Directory.CreateDirectory(pathHelper);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                if (!await DeleteDirectoriesForUser(privateFolderPath, cloudUser))
                {
                    throw new CloudLoggerException($"Directory not removeable or data remained in database : {privateFolderPath}, {cloudUser.UserName}");
                }

                return false; //if anything can not be created -> fail the whole task
            }
        }

        public async Task<bool> DeleteDirectoriesForUser(string privateFolderPath, CloudUser cloudUser)
        {
            try
            {
                if (!Directory.Exists(privateFolderPath)) //deleting user folder
                {
                    Directory.Delete(privateFolderPath, true);
                }

                var userFiles = await context.SharedFiles.Where(x => x.Owner == cloudUser).ToListAsync(); //deleting user files and folders
                var userFolders = await context.SharedFolders.Where(x => x.Owner == cloudUser).ToListAsync();

                context.SharedFiles.RemoveRange(userFiles);
                context.SharedFolders.RemoveRange(userFolders);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> CreateDirectory(string folderName, string cloudPath, CloudUser user)
        {
            if (String.IsNullOrWhiteSpace(folderName)) //checking restrictions
                throw new CloudFunctionStopException("no directory name");

            if (!Regex.IsMatch(folderName, Constants.FolderAndFileRegex))
                throw new CloudFunctionStopException("invalid directory name");

            if (String.IsNullOrWhiteSpace(cloudPath))
                throw new CloudFunctionStopException("invalid path");

            DirectoryInfo di = new DirectoryInfo(Path.Combine(ParseRootName(cloudPath), folderName));

            try
            {
                if (!di.Exists)
                {
                    await Task.Run(() => di.Create()); //creating folder

                    Pair<string, string> parentPathAndName = GetParentPathAndName(cloudPath); //add sharing to folder (inherits parent sharing)

                    Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                    if (!await SetDirectoryConnectedState(cloudPath, folderName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, connections.First, connections.Second))
                        throw new CloudFunctionStopException("failed to adjust folder rights");

                    return di.Name;
                }
                else
                {
                    throw new CloudFunctionStopException("directory already exists");
                }
            }
            catch (Exception)
            {
                try
                {
                    if (!(await RemoveDirectory(folderName, cloudPath, user)))
                    {
                        throw new CloudLoggerException($"Directory not removeable : {Path.Combine(ParseRootName(cloudPath), folderName)}"); //logging serious problem
                    }
                }
                catch (Exception ex)
                {
                    throw new CloudLoggerException($"Directory not removeable : {Path.Combine(ParseRootName(cloudPath), folderName)} {ex.Message}"); //logging serious problem
                }

                throw new CloudFunctionStopException("error while adding directory");
            }
        }

        public async Task<bool> RemoveDirectory(string folderName, string cloudPath, CloudUser user)
        {
            if (String.IsNullOrWhiteSpace(folderName)) //check for constraints
            {
                throw new CloudFunctionStopException("invalid folder name");
            }

            DirectoryInfo di = new DirectoryInfo(Path.Combine(ParseRootName(cloudPath), folderName));

            if (!di.Exists)
            {
                throw new CloudFunctionStopException("directory does not exist");
            }

            if (IsSystemFolder(di.FullName))
            {
                return false;
            }

            try
            {
                await Task.Run(() => di.Delete(true)); //delete folder

                await SetObjectAndUnderlyingObjectsState(cloudPath, folderName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, false, false); //delete folder from database
            }
            catch (Exception)
            {
                throw new CloudFunctionStopException("error while removing directory");
            }

            return true;
        }

        public async Task<string> CreateFile(IFormFile file, string cloudPath, CloudUser user)
        {
            if (file is null) //checking restrictions
                throw new CloudFunctionStopException("no file");

            if (!Regex.IsMatch(file.FileName, Constants.FolderAndFileRegex))
                throw new CloudFunctionStopException("invalid file name");

            if (String.IsNullOrWhiteSpace(cloudPath))
                throw new CloudFunctionStopException("invalid path");

            string path = ParseRootName(cloudPath);
            string newName = new string(file.FileName);

            try
            {
                FileInfo fi = new FileInfo(Path.Combine(path, RenameObject(path, ref newName, true)));

                using (FileStream stream = fi.Create())
                {
                    await file.CopyToAsync(stream);
                }

                fi.SetAccessControl(SecurityManager.GetFileRights());

                Pair<string, string> parentPathAndName = GetParentPathAndName(cloudPath);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                if (!await SetFileConnectedState(cloudPath, newName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, connections.First, connections.Second))
                    throw new CloudFunctionStopException("failed to adjust file rights");

                return newName;
            }
            catch (Exception)
            {
                try
                {
                    if (!(await RemoveFile(path, newName, user)))
                    {
                        throw new CloudLoggerException($"File not removeable : {Path.Combine(ParseRootName(cloudPath), newName)}");
                    }
                }
                catch (Exception ex)
                {
                    throw new CloudLoggerException($"File not removeable : {Path.Combine(ParseRootName(cloudPath), newName)} {ex.Message}");
                }

                throw new CloudFunctionStopException("error while adding file");
            }
        }

        public async Task<bool> RemoveFile(string fileName, string cloudPath, CloudUser user)
        {

            if (String.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("invalid folder name");
            }

            FileInfo fi = new FileInfo(Path.Combine(ParseRootName(cloudPath), fileName));

            if (!fi.Exists)
            {
                throw new CloudFunctionStopException("file does not exist");
            }

            try
            {
                await Task.Run(() => fi.Delete());

                await SetFileConnectedState(cloudPath, fileName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, false, false);
            }
            catch
            {
                throw new CloudFunctionStopException("error while removing file");
            }

            return true;
        }

        public async Task<List<CloudFile>> GetCurrentDepthCloudFiles(string cloudPath, bool connectedToApp = false, bool connectedToWeb = false, string? pattern = null)
        {
            try
            {
                string path = ParseRootName(cloudPath);

                var appsharedfiles = context.SharedFiles.Where(x => x.ConnectedToApp && x.CloudPathFromRoot == cloudPath).Select(x => x.Name.ToLower()).ToList() ?? new();
                var websharedfiles = context.SharedFiles.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == cloudPath).Select(x => x.Name.ToLower()).ToList() ?? new();

                var files = pattern is null ? Directory.GetFiles(path) : Directory.GetFiles(path, pattern);

                var items = await Task.FromResult<IEnumerable<CloudFile>>(files.Select(x => new CloudFile(new FileInfo(x), appsharedfiles.Contains(Path.GetFileName(x)?.ToLower()!), websharedfiles.Contains(Path.GetFileName(x)?.ToLower()!), Path.Combine(cloudPath, Path.GetFileName(x)))).OrderBy(x => x.Info.Name));

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
                throw new CloudFunctionStopException("error occurred while getting files");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string currentPath, bool connectedToApp = false, bool connectedToWeb = false, string? pattern = null)
        {
            try
            {
                string path = ParseRootName(currentPath);

                var appsharedfolders = await context.SharedFolders.Where(x => x.ConnectedToApp && x.CloudPathFromRoot == currentPath).Select(x => x.Name.ToLower()).ToListAsync() ?? new();
                var websharedfolders = await context.SharedFolders.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == currentPath).Select(x => x.Name.ToLower()).ToListAsync() ?? new();

                var folders = pattern is null ? Directory.GetDirectories(path) : Directory.GetDirectories(path, pattern);

                var items = await Task.FromResult<IEnumerable<CloudFolder>>(folders.Select(x => new CloudFolder(new DirectoryInfo(x), appsharedfolders.Contains(Path.GetFileName(x)?.ToLower()!), websharedfolders.Contains(Path.GetFileName(x)?.ToLower()!), Path.Combine(currentPath, Path.GetFileName(x)))).OrderBy(x => x.Info.Name));

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
                throw new CloudFunctionStopException("error occurred while getting folders");
            }
        }

        public string ChangeRootName(string path)
        {
            if (path.StartsWith(Constants.PrivateRootName))
            {
                return path.Replace(Constants.PrivateRootName, Constants.PublicRootName);
            }
            else if (path.StartsWith(Constants.PublicRootName))
            {
                return path.Replace(Constants.PublicRootName, Constants.PrivateRootName);
            }

            return path;
        }

        public string ServerPath(string cloudPath)
        {
            return ParseRootName(cloudPath);
        }

        public async Task<DirectoryInfo> GetFolderByPath(string serverPath, string folderName)
        {
            return await Task.FromResult<DirectoryInfo>(new DirectoryInfo(Directory.GetDirectories(serverPath, folderName).First()));
        }

        #endregion

        private async Task<bool> SetDirectoryConnectedState(string cloudPath, string directoryName, string sharingPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ParseRootName(cloudPath));

                DirectoryInfo? entryInfo = di.GetDirectories().FirstOrDefault(x => x.Name.ToLower() == directoryName.ToLower());

                if ((connectToApp == true || connectToWeb == true) && (entryInfo is null || !entryInfo.Exists))
                {
                    throw new InvalidDataException("Directory does not exists!");
                }

                SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot.ToLower() == cloudPath.ToLower() && x.Name.ToLower() == directoryName.ToLower() && x.Owner == user);

                if (sharedFolder is null)
                {
                    sharedFolder = new SharedFolder
                    {
                        Name = entryInfo?.Name ?? directoryName,
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
                DirectoryInfo di = new DirectoryInfo(ParseRootName(currentPath));

                DirectoryInfo? entryInfo = di.GetDirectories().FirstOrDefault(x => x.Name.ToLower() == directoryName.ToLower());

                if (entryInfo is null)
                    throw new InvalidDataException("part of path does not exist");

                Queue<Tuple<string, string, DirectoryInfo>> underlyingDirectories = new Queue<Tuple<string, string, DirectoryInfo>>(new List<Tuple<string, string, DirectoryInfo>>() { new Tuple<string, string, DirectoryInfo>(currentPath, dbPath, new DirectoryInfo(Path.Combine(ParseRootName(currentPath), entryInfo.Name))) });

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
                DirectoryInfo di = new DirectoryInfo(ParseRootName(cloudPath));

                FileInfo? entryInfo = di.GetFiles().FirstOrDefault(x => x.Name.ToLower() == fileName.ToLower());

                if ((connectToApp == true || connectToWeb == true) && (entryInfo is null || !entryInfo.Exists))
                {
                    throw new InvalidDataException("File does not exists!");
                }

                SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot.ToLower() == cloudPath.ToLower() && x.Name.ToLower() == fileName.ToLower() && x.Owner == user);

                if (sharedFile is null)
                {
                    sharedFile = new SharedFile
                    {
                        Name = entryInfo?.Name ?? fileName,
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

            return (await context.SharedFiles.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == currentPath).ToListAsync()).Select(x => new CloudFile(new FileInfo(Path.Combine(ParseRootName(x.CloudPathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb, String.Empty)).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList() ?? new();
        }

        public async Task<List<CloudFolder>> GetCurrentDepthAppSharingDirectories(string currentPath)
        {
            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(currentPath));

            return (await context.SharedFolders.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == currentPath).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(ParseRootName(x.CloudPathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb, String.Empty)).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList() ?? new();
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
                return (await context.SharedFiles.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == path).ToListAsync()).Select(x => new CloudFile(new FileInfo(ParseRootName(Path.Combine(x.CloudPathFromRoot, x.Name))), false, true, String.Empty)).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList();
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
                return (await context.SharedFolders.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == path).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(ParseRootName(Path.Combine(x.CloudPathFromRoot, x.Name))), false, true, String.Empty)).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList();
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

                if (Directory.Exists(newFolderPathAndName) && folderName.ToLower() != newName.ToLower())
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

                if (File.Exists(Path.Combine(filePath, newFileName)) && fileName.ToLower() != newName.ToLower())
                    RenameObject(filePath, ref newFileName, true);

                File.Move(Path.Combine(filePath, fileName), Path.Combine(filePath, newFileName));

                SharedFile? file = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == currentPath && x.Name == fileName);

                if (file is not null)
                {
                    file.Name = newFileName;

                    context.SharedFiles.Update(file);
                    await context.SaveChangesAsync();
                }

                return newFileName;
            }
            catch (Exception)
            {
                throw new FileLoadException("Error while renaming file");
            }
        }

        public async Task<string> CopyFile(string? source, string destination, ClaimsPrincipal userPrincipal)
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

                File.Copy(src, Path.Combine(dest, RenameObject(dest, ref name, true)));

                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                Pair<string, string> parentPathAndName = GetParentPathAndName(destination);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                await SetFileConnectedState(destination, name, ChangeOwnerIdentification(ChangeRootName(destination), user.UserName), user, connections.First, connections.Second);

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

        public async Task<string> CopyFolder(string? source, string destination, ClaimsPrincipal userPrincipal)
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

                DirectoryInfo di = new DirectoryInfo(src);

                string name = new string(di.Name);
                string newDirectoryPath = Path.Combine(dest, RenameObject(dest, ref name, false));

                Directory.CreateDirectory(newDirectoryPath);

                Queue<DirectoryInfo> dirData = new Queue<DirectoryInfo>(new DirectoryInfo[] { di });

                while (dirData.Any())
                {
                    DirectoryInfo directory = dirData.Dequeue();

                    newDirectoryPath = directory.FullName.Replace(src, newDirectoryPath);

                    if (!Directory.Exists(newDirectoryPath))
                    {
                        Directory.CreateDirectory(newDirectoryPath);
                    }

                    foreach (FileInfo fi in directory.GetFiles())
                    {
                        File.Copy(fi.FullName, Path.Combine(newDirectoryPath, fi.Name));
                    }

                    foreach (DirectoryInfo dir in directory.GetDirectories())
                    {
                        dirData.Enqueue(dir);
                    }
                }

                CloudUser user = await userManager.GetUserAsync(userPrincipal);

                Pair<string, string> parentPathAndName = GetParentPathAndName(destination);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                await SetObjectAndUnderlyingObjectsState(destination, name, ChangeOwnerIdentification(ChangeRootName(destination), user.UserName), user, connections.First, connections.Second);

                if (name == di.Name)
                {
                    return await Task.FromResult<string>(String.Empty);
                }

                return await Task.FromResult<string>(name);
            }
            catch (Exception)
            {
                throw new Exception("Error while pasting directory");
            }
        }

        public async Task<CloudPathData> GetSessionCloudPathData()
        {
            CloudPathData data = null!;
            if (httpContext.HttpContext!.Session.Keys.Contains(Constants.CloudCookieKey))
            {
                data = JsonSerializer.Deserialize<CloudPathData>(httpContext.HttpContext!.Session.GetString(Constants.CloudCookieKey)!)!;
            }
            else
            {
                CloudUser? user = await userManager.GetUserAsync(httpContext.HttpContext!.User);
                data = new CloudPathData();
                data.SetDefaultPathData(user?.Id.ToString());
                await SetSessionCloudPathData(data);
            }
            return data;
        }

        public Task<bool> SetSessionCloudPathData(CloudPathData pathData)
        {
            if (pathData == null)
                return Task.FromResult<bool>(false);

            httpContext.HttpContext!.Session.SetString(Constants.CloudCookieKey, JsonSerializer.Serialize<CloudPathData>(pathData));

            return Task.FromResult<bool>(true);
        }

        public async Task<string> ChangeToDirectory(string path)
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            if (path.StartsWith(Constants.AbsolutePathMarker))
            {
                if (Directory.Exists(ParseRootName(path)))
                {
                    try
                    {
                        await pathData.SetPath(path, ParseRootName(Constants.PrivateRootName));

                        await SetSessionCloudPathData(pathData);

                        return await Task.FromResult<string>(pathData.CurrentPathShow);
                    }
                    catch (Exception)
                    {
                        throw new InvalidDataException("unable to modify path");
                    }
                }

                throw new InvalidDataException($"part of path does not exist");
            }
            else
            {
                if (path == String.Empty)
                {
                    return await Task.FromResult<string>(pathData.CurrentPathShow);
                }

                string[] pathElements = path.Split(Path.DirectorySeparatorChar);

                foreach (string element in pathElements)
                {
                    if (element == String.Empty)
                    {
                        throw new InvalidDataException($"part of path does not exist (empty path element)");
                    }

                    if (element == Constants.DirectoryBack)
                    {
                        pathData.RemoveFolderFromPrevDirs();
                    }
                    else
                    {
                        DirectoryInfo di = new DirectoryInfo(ParseRootName(pathData.TrySetFolder(element) ?? String.Empty));

                        if (di.Exists)
                        {
                            if (di.Parent is not null && di.Parent.Exists)
                            {
                                di = di.Parent.GetDirectories().FirstOrDefault(x => x.Name.Equals(element, StringComparison.OrdinalIgnoreCase)) ?? di;
                            }


                            if (Directory.Exists(ParseRootName(pathData.TrySetFolder(di.Name) ?? String.Empty)))
                            {
                                pathData.SetFolder((di.Name));
                            }
                            else
                            {
                                throw new InvalidDataException($"part of path does not exist");
                            }
                        }
                        else
                        {
                            throw new InvalidDataException($"part of path does not exist");
                        }
                    }
                }
                await SetSessionCloudPathData(pathData);

                return await Task.FromResult<string>(pathData.CurrentPathShow);
            }
        }

        public async Task<string> ListCurrentSubDirectories()
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            StringBuilder sb = new StringBuilder();

            int counter = 0;

            sb.Append("Directories:\n");
            sb.Append("Created time      Size        Shared in app  Shared on web  Name\n");
            sb.Append("----------------  ----------  -------------  -------------  ------\n\n");

            foreach (var dir in await GetCurrentDepthCloudDirectories(pathData.CurrentPath))
            {
                sb.Append(dir.ToString());

                ++counter;
            }

            sb.Append('\n');

            counter = 0;

            sb.Append("Files:\n");
            sb.Append("Created time      Size        Shared in app  Shared on web  Name\n");
            sb.Append("----------------  ----------  -------------  -------------  ------\n\n");

            foreach (var file in await GetCurrentDepthCloudFiles(pathData.CurrentPath))
            {
                sb.Append(file.ToString());

                ++counter;
            }

            return sb.ToString();
        }

        public async Task<string> GetTerminalHelpText()
        {
            JArray commands = JArray.Parse(File.ReadAllText(Constants.TerminalCommandsDataFilePath));

            return await Task.FromResult<string>((String.Join('\n', commands?.Select(x => $"{Constants.TerminalYellowText(x[Constants.TerminalHelpName]?.ToString() ?? String.Empty)} : {x[Constants.TerminalHelpDescription]?.ToString() ?? String.Empty}") ?? Array.Empty<string>()) ?? Constants.TerminalRedText("no command data available")) + Environment.NewLine);
        }

        public async Task<string> PrintWorkingDirectory()
        {
            return (await GetSessionCloudPathData()).CurrentPathShow;
        }

        public async Task<List<CloudFolder>> SearchDirectoryInCurrentDirectory(string currentPath, string pattern)
        {
            return await GetCurrentDepthCloudDirectories(currentPath, pattern: pattern);
        }

        public async Task<List<CloudFile>> SearchFileInCurrentDirectory(string currentPath, string pattern)
        {
            return await GetCurrentDepthCloudFiles(currentPath, pattern: pattern);
        }

        #region Private Methods

        /// <summary>
        /// Method to rename a file or folder to a non-existent in the actual folder
        /// </summary>
        /// <param name="actualPath">path to the folder or file (on disk)</param>
        /// <param name="name">name of file, folder (due to ref keyword, it will be modified)</param>
        /// <param name="isFile">Boolean value if passed data refer to file</param>
        /// <returns>The modified name</returns>
        private string RenameObject(string actualPath, ref string name, bool isFile)
        {
            int counter = 0;

            string pathAndName = Path.Combine(actualPath, name);

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

                    pathAndName = Path.Combine(actualPath, name);
                }
            }
            else
            {
                string nameBase = new string(name);

                while (System.IO.Directory.Exists(pathAndName))
                {
                    name = nameBase + Constants.FileNameDelimiter + (++counter).ToString();

                    pathAndName = Path.Combine(actualPath, name);
                }
            }

            return name;
        }

        /// <summary>
        /// Method to add an existing file to the zipArchive
        /// </summary>
        /// <param name="archive">Archive for the file to be added to</param>
        /// <param name="cloudPath">Path to the file in app</param>
        /// <param name="name">Name of the file</param>
        /// <param name="connectedToApp">Parameter to filter only app connected files (default false)</param>
        /// <param name="connectedToWeb">Parameter to filter only web connected files (default false)</param>
        /// <returns>Task (need to be awaited)</returns>
        private async Task<bool> AddFileToArchive(ZipArchive archive, string cloudPath, string name, bool connectedToApp = false, bool connectedToWeb = false)
        {
            try
            {
                FileInfo fi = new FileInfo(Path.Combine(ParseRootName(cloudPath), name));

                if (!fi.Exists)
                    throw new FileNotFoundException("File does not exist");

                if (connectedToApp)
                {
                    var connections = await FileIsSharedInAppInWeb(cloudPath, name);

                    if (connections.First)
                    {
                        archive.CreateEntryFromFile(fi.FullName, name);
                    }
                }
                else if (connectedToWeb)
                {
                    var connections = await FileIsSharedInAppInWeb(cloudPath, name);

                    if (connections.Second)
                    {
                        archive.CreateEntryFromFile(fi.FullName, name);
                    }
                }
                else
                {
                    archive.CreateEntryFromFile(fi.FullName, name);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to get folder is shared in app and in web from database
        /// </summary>
        /// <param name="cloudPath">Path to directory in app</param>
        /// <param name="directoryName">Name of directory to be searched</param>
        /// <returns>A pair indicating if the folder is shared in app and in web in this order</returns>
        private async Task<Pair<bool, bool>> FolderIsSharedInAppInWeb(string cloudPath, string directoryName)
        {
            SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == directoryName);

            if (sharedFolder is null)
            {
                return new Pair<bool, bool>(false, false);
            }

            return new Pair<bool, bool>(sharedFolder.ConnectedToApp, sharedFolder.ConnectedToWeb);
        }

        /// <summary>
        /// Method to get folder is shared in app and in web from database
        /// </summary>
        /// <param name="cloudPath">Path to directory in app</param>
        /// <param name="directoryName">Name of directory to be searched</param>
        /// <returns>A pair indicating if the folder is shared in app and in web in this order</returns>

        private async Task<Pair<bool, bool>> FileIsSharedInAppInWeb(string cloudPath, string directoryName)
        {
            SharedFile? sharedFile = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == directoryName);

            if (sharedFile is null)
            {
                return new Pair<bool, bool>(false, false);
            }

            return new Pair<bool, bool>(sharedFile.ConnectedToApp, sharedFile.ConnectedToWeb);
        }

        /// <summary>
        /// Method to split the path into the last folder and the remaining path string
        /// </summary>
        /// <param name="path">The path to be splitted</param>
        /// <returns>A pair containing the path to the last folder and the folder</returns>
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

        /// <summary>
        /// Method to parse cloud path to physical path
        /// </summary>
        /// <param name="cloudPath">Path in the app</param>
        /// <returns>The psysical path</returns>
        private string ParseRootName(string cloudPath)
        {
            if (cloudPath.StartsWith(Constants.PrivateRootName))
            {
                return cloudPath.Replace(Constants.PrivateRootName, Constants.GetPrivateBaseDirectory());
            }

            return String.Empty;
        }

        /// <summary>
        /// Method to check if folder is system folder (no remove, no rename)
        /// </summary>
        /// <param name="physicalPath">Path to the folder</param>
        /// <returns></returns>
        private bool IsSystemFolder(string physicalPath)
        {
            List<string> pathFolders = physicalPath.Split(Path.DirectorySeparatorChar).ToList();

            if (pathFolders.Count < 1)
                return false;

            return Constants.SystemFolders.Contains(pathFolders.Last()) && ((pathFolders.Count - pathFolders.FindIndex(x => x == Constants.WebRootFolderName) - 1) == Constants.DistanceToRootFolder);
        }

        #endregion
    }
}
