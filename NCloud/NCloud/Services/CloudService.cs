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
using System.IO;
using NCloud.Models.Extensions;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;

        public CloudService(CloudDbContext context)
        {
            this.context = context;
        }

        #region Public Instance Methods 
        public async Task<CloudUser?> GetAdmin()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.UserName == Constants.AdminUserName);
        }
        public async Task<bool> RemoveUser(CloudUser user)
        {
            try
            {
                context.Users.Remove(user);

                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
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
                double dirSize = await GetDirectorySize(cloudPath);

                if (!await SetObjectAndUnderlyingObjectsState(cloudPath, folderName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, false, false, true) //delete folder from database
                    && await SetObjectAndUnderlyingObjectsState(cloudPath, folderName, Constants.GetSharingRootPathInDatabase(user.UserName), user, false, false, true))
                    throw new CloudFunctionStopException("failed to remove folder connectivity");

                await Task.Run(() => di.Delete(true)); //delete folder

                await UpdateUserStorageUsed(user, (-1) * dirSize);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
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

            if (user.UsedSpace + file.Length > user.MaxSpace)
                throw new CloudFunctionStopException("not enough space");

            string path = ParseRootName(cloudPath);
            string newName = new string(file.FileName);

            try
            {
                FileInfo fi = new FileInfo(Path.Combine(path, RenameObject(path, ref newName, true)));

                using (FileStream stream = fi.Create())
                {
                    await file.CopyToAsync(stream);
                }

                await UpdateUserStorageUsed(user, fi.Length);

                fi.SetAccessControl(SecurityManager.GetFileRights(fi)); //file does not have execution right

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
                    if (!(await RemoveFile(newName, cloudPath, user)))
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
                double fileSize = fi.Length;

                if (!await SetFileConnectedState(cloudPath, fileName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, false, false, true)
                    && !await SetFileConnectedState(cloudPath, fileName, Constants.GetSharingRootPathInDatabase(user.UserName), user, false, false, true))
                    throw new CloudFunctionStopException("failed to adjust file connectivity");

                await Task.Run(() => fi.Delete());

                await UpdateUserStorageUsed(user, (-1) * fileSize);

            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                throw new CloudFunctionStopException("error while removing file");
            }

            return true;
        }

        public async Task<List<CloudFile>> GetCurrentDepthCloudFiles(string cloudPath, bool connectedToApp = false, bool connectedToWeb = false, string? pattern = null)
        {
            try
            {
                DirectoryInfo parentDir = new DirectoryInfo(ParseRootName(cloudPath));

                if (!parentDir.Exists)
                    throw new CloudFunctionStopException("parent directory does not exist");

                var sharedFiles = context.SharedFiles.Where(x => x.CloudPathFromRoot == cloudPath).ToList() ?? new();

                var files = pattern is null ? parentDir.GetFiles() : parentDir.GetFiles(pattern);

                var items = await Task.FromResult<IEnumerable<CloudFile>>(files.Select(x =>
                {
                    SharedFile? sharedFile = sharedFiles.FirstOrDefault(y => y.Name == x.Name);

                    return new CloudFile(x, sharedFile?.ConnectedToApp ?? false, sharedFile?.ConnectedToWeb ?? false, sharedFile?.Id.ToString() ?? String.Empty);

                }).OrderBy(x => x.Info.Name));

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
            catch (Exception)
            {
                throw new CloudFunctionStopException("error occurred while getting files");
            }
        }

        public async Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string cloudPath, bool connectedToApp = false, bool connectedToWeb = false, string? pattern = null)
        {
            try
            {
                DirectoryInfo parentDir = new DirectoryInfo(ParseRootName(cloudPath));

                if (!parentDir.Exists)
                    throw new CloudFunctionStopException("parent directory does not exist");

                var sharedFolders = context.SharedFolders.Where(x => x.CloudPathFromRoot == cloudPath).ToList() ?? new();

                var folders = pattern is null ? parentDir.GetDirectories() : parentDir.GetDirectories(pattern);

                var items = await Task.FromResult<IEnumerable<CloudFolder>>(folders.Select(x =>
                {
                    SharedFolder? sharedFolder = sharedFolders.FirstOrDefault(y => y.Name == x.Name);

                    return new CloudFolder(x, sharedFolder?.ConnectedToApp ?? false, sharedFolder?.ConnectedToWeb ?? false, sharedFolder?.Id.ToString() ?? String.Empty);

                }).OrderBy(x => x.Info.Name));

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
            catch (Exception)
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

        public async Task<DirectoryInfo> GetFolderByPath(string physicalPath, string folderName)
        {
            DirectoryInfo di = new DirectoryInfo(physicalPath);

            if (!di.Exists)
                throw new CloudFunctionStopException("Parent does not exist");

            DirectoryInfo? searchedDir = di.GetDirectories().FirstOrDefault(x => x.Name.ToLower() == folderName.ToLower());

            if (searchedDir is null || !searchedDir.Exists)
                throw new CloudFunctionStopException("Directory does not exist");

            return await Task.FromResult<DirectoryInfo>(searchedDir);
        }

        public async Task<bool> ConnectDirectoryToWeb(string cloudPath, string directoryName, CloudUser user)
        {

            try
            {
                return await SetObjectAndUnderlyingObjectsState(cloudPath, directoryName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToWeb: true);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectDirectoryToApp(string cloudPath, string directoryName, CloudUser user)
        {
            try
            {
                return await SetObjectAndUnderlyingObjectsState(cloudPath, directoryName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, connectToApp: true, useSharingPath: true);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectDirectoryFromApp(string cloudPath, string directoryName, CloudUser user)
        {
            try
            {
                return await SetObjectAndUnderlyingObjectsState(cloudPath, directoryName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, connectToApp: false, useSharingPath: true);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectDirectoryFromWeb(string cloudPath, string directoryName, CloudUser user)
        {
            try
            {
                return await SetObjectAndUnderlyingObjectsState(cloudPath, directoryName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToWeb: false);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectFileToApp(string cloudPath, string fileName, CloudUser user)
        {
            try
            {
                return await SetFileConnectedState(cloudPath, fileName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, connectToApp: true, useSharingPath: true);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ConnectFileToWeb(string cloudPath, string fileName, CloudUser user)
        {
            try
            {
                return await SetFileConnectedState(cloudPath, fileName, Constants.GetSharingRootPathInDatabase(user.UserName), user, connectToWeb: true);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectFileFromApp(string cloudPath, string fileName, CloudUser user)
        {
            try
            {
                return await SetFileConnectedState(cloudPath, fileName, ChangeOwnerIdentification(ChangeRootName(cloudPath), user.UserName), user, connectToApp: false, useSharingPath: true);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DisconnectFileFromWeb(string cloudPath, string fileName, CloudUser user)
        {
            try
            {
                return await SetFileConnectedState(cloudPath, fileName, cloudPath, user, connectToWeb: false);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CloudFile>> GetCurrentDepthAppSharingFiles(string sharedPath)
        {
            if (sharedPath == Constants.PublicRootName)
                return await Task.FromResult<List<CloudFile>>(new List<CloudFile>());

            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(sharedPath));

            return (await context.SharedFiles.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == sharedPath).ToListAsync()).Select(x => new CloudFile(new FileInfo(Path.Combine(ParseRootName(x.CloudPathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb, String.Empty, x.CloudPathFromRoot.Slice(Constants.PrivateRootName.Length, Constants.PrivateRootName.Length + Constants.GuidLength + 1).Replace(System.IO.Path.DirectorySeparatorChar, Constants.PathSeparator))).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList() ?? new();
        }

        public async Task<List<CloudFolder>> GetCurrentDepthAppSharingDirectories(string sharedPath)
        {
            if (sharedPath == Constants.PublicRootName)
                return await context.Users.Where(x => x.SharedFiles.Where(x => x.ConnectedToApp).Count() > 0 || x.SharedFolders.Where(x => x.ConnectedToApp).Count() > 0).Select(x => new CloudFolder(x.UserName, null)).ToListAsync();

            CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.UserName == GetSharedPathOwnerUser(sharedPath));

            return (await context.SharedFolders.Where(x => x.ConnectedToApp && x.Owner == user && x.SharedPathFromRoot == sharedPath).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(ParseRootName(x.CloudPathFromRoot), x.Name)), x.ConnectedToApp, x.ConnectedToWeb, String.Empty, x.CloudPathFromRoot.Slice(Constants.PrivateRootName.Length, Constants.PrivateRootName.Length + Constants.GuidLength + 1).Replace(System.IO.Path.DirectorySeparatorChar, Constants.PathSeparator))).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList() ?? new();
        }

        public async Task<bool> OwnerOfPathIsActualUser(string sharedPath, CloudUser user)
        {
            try
            {
                return await Task.FromResult<bool>(GetSharedPathOwnerUser(sharedPath) == user.UserName);
            }
            catch
            {
                return await Task.FromResult<bool>(false);
            }
        }

        public async Task<List<SharedFolder>> GetUserWebSharedFolders(CloudUser user)
        {
            return await context.SharedFolders.Where(x => x.Owner == user && x.ConnectedToWeb).OrderBy(x => x.CloudPathFromRoot).ThenBy(x => x.Name).ToListAsync();
        }

        public async Task<List<SharedFile>> GetUserWebSharedFiles(CloudUser user)
        {
            return await context.SharedFiles.Where(x => x.Owner == user && x.ConnectedToWeb).OrderBy(x => x.CloudPathFromRoot).ThenBy(x => x.Name).ToListAsync();
        }

        public async Task<string> WebBackCheck(string path)
        {
            try
            {
                Pair<string, string> parentPathAndName = GetParentPathAndName(path);

                if (await FolderIsSharedInWeb(parentPathAndName.First, parentPathAndName.Second))
                {
                    return parentPathAndName.First;
                }

                return path;
            }
            catch (Exception)
            {
                return path;
            }
        }

        public async Task<List<CloudFile>> GetCurrentDepthWebSharingFiles(string path)
        {
            return (await context.SharedFiles.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == path).ToListAsync()).Select(x => new CloudFile(new FileInfo(ParseRootName(Path.Combine(x.CloudPathFromRoot, x.Name))), false, true, String.Empty)).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList();
        }

        public async Task<List<CloudFolder>> GetCurrentDepthWebSharingDirectories(string path)
        {
            return (await context.SharedFolders.Where(x => x.ConnectedToWeb && x.CloudPathFromRoot == path).ToListAsync()).Select(x => new CloudFolder(new DirectoryInfo(ParseRootName(Path.Combine(x.CloudPathFromRoot, x.Name))), false, true, String.Empty)).Where(x => x.Info.Exists).OrderBy(x => x.Info.Name).ToList();
        }

        public async Task<string?> CreateZipFile(List<string> itemsForDownload, string cloudPath, string filePath, bool connectedToApp, bool connectedToWeb)
        {
            using var zipFile = System.IO.File.Create(filePath);

            try
            {
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    foreach (string itemName in itemsForDownload!)
                    {
                        if (itemName != Constants.NotSelectedResult && !String.IsNullOrWhiteSpace(itemName))
                        {
                            if (itemName.StartsWith(Constants.SelectedFileStarterSymbol))
                            {
                                try
                                {
                                    string name = itemName[1..];

                                    await AddFileToArchive(archive, cloudPath, name, connectedToApp, connectedToWeb);
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
                                    string serverPathStart = ParseRootName(cloudPath);
                                    string currentRelativePath = String.Empty;

                                    Queue<Pair<string, DirectoryInfo>> directories = new(new List<Pair<string, DirectoryInfo>>() { new Pair<string, DirectoryInfo>(currentRelativePath, await GetFolderByPath(serverPathStart, name)) });

                                    while (directories.Any())
                                    {
                                        var directoryInfo = directories.Dequeue();

                                        int counter = 0;

                                        currentRelativePath = Path.Combine(directoryInfo.First, directoryInfo.Second.Name);

                                        try
                                        {
                                            foreach (CloudFile file in await GetCurrentDepthCloudFiles(Path.Combine(cloudPath, currentRelativePath), connectedToApp, connectedToWeb))
                                            {
                                                try
                                                {
                                                    archive.CreateEntryFromFile(Path.Combine(serverPathStart, currentRelativePath, file.Info.Name), Path.Combine(currentRelativePath, file.Info.Name));

                                                    ++counter;
                                                }
                                                catch (Exception) { }
                                            }
                                        }
                                        catch (Exception) { }

                                        try
                                        {
                                            foreach (CloudFolder folder in await GetCurrentDepthCloudDirectories(Path.Combine(cloudPath, currentRelativePath), connectedToApp, connectedToWeb))
                                            {

                                                directories.Enqueue(new Pair<string, DirectoryInfo>(currentRelativePath, folder.Info));

                                                ++counter;
                                            }
                                        }
                                        catch (Exception) { }

                                        if (counter == 0)
                                        {
                                            try
                                            {
                                                archive.CreateEntry(currentRelativePath).ExternalAttributes = Constants.EmptyFolderAttributeNumberZip;
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception($"{ex.Message} ({itemName})");
                                }
                            }
                        }
                    }
                }

                return filePath;
            }
            catch (Exception)
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

                throw new CloudFunctionStopException("Error while creating zip file");
            }
        }

        public async Task<string> ChangePathStructure(string path)
        {
            try
            {
                string[] pathElements = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

                if (pathElements.Length < 2)
                {
                    return path;
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
                        return path;
                    }
                }

                if (pathElements[Constants.RootProviderPlaceinPath] == Constants.PrivateRootName)
                {
                    CloudUser? user = await context.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(pathElements[Constants.OwnerPlaceInPath]));

                    if (user is not null)
                    {
                        pathElements[Constants.RootProviderPlaceinPath] = Constants.PublicRootName;
                        pathElements[Constants.OwnerPlaceInPath] = user.UserName;

                        return String.Join(Path.DirectorySeparatorChar, pathElements);
                    }
                    else
                    {
                        return path;
                    }
                }

                return path;
            }
            catch (Exception)
            {
                return path;
            }
        }

        public async Task<bool> ModifyFileContent(string file, string content, CloudUser user)
        {
            try
            {
                string filePath = ParseRootName(file);

                FileInfo fi = new FileInfo(filePath);

                if (!fi.Exists)
                    throw new FileNotFoundException("File does not exist");

                double contentSize = GetStringLengthInBytes(content);

                contentSize -= fi.Length; //calculate the difference

                if (user.UsedSpace + contentSize > user.MaxSpace)
                {
                    throw new CloudFunctionStopException("storage for user is on full, changes can not be saved");
                }

                File.WriteAllText(filePath, content);

                await UpdateUserStorageUsed(user, contentSize);

                return true;
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                throw new FileNotFoundException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<CloudFolder> GetFolder(string cloudPath, string folderName)
        {
            var folder = await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == folderName);

            return await Task.FromResult<CloudFolder>(new CloudFolder(new DirectoryInfo(Path.Combine(ParseRootName(cloudPath), folderName)), folder?.ConnectedToApp ?? false, folder?.ConnectedToWeb ?? false, Path.Combine(cloudPath, folderName)));
        }

        public async Task<CloudFile> GetFile(string currentPath, string fileName)
        {
            var file = await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot == currentPath && x.Name == fileName);

            return await Task.FromResult<CloudFile>(new CloudFile(new FileInfo(Path.Combine(ParseRootName(currentPath), fileName)), file?.ConnectedToApp ?? false, file?.ConnectedToWeb ?? false, Path.Combine(currentPath, fileName)));
        }

        public async Task<string> RenameFolder(string cloudPath, string folderName, string newName, SharedPathData sharedData)
        {
            string folderPath = ParseRootName(cloudPath);
            string folderPathAndName = Path.Combine(folderPath, folderName);

            if (!Directory.Exists(folderPathAndName))
                throw new CloudFunctionStopException("source directory does not exist");

            if (IsSystemFolder(folderPathAndName))
                throw new CloudFunctionStopException("system directories can not be renamed");

            string newFolderPathAndName = Path.Combine(folderPath, newName);

            if (Directory.Exists(newFolderPathAndName) && folderName.ToLower() != newName.ToLower())
                throw new CloudFunctionStopException("directory with this name already exists");

            string? oldSharingPathAndName = null!;
            string? newSharingPathAndName = null!;

            Directory.Move(folderPathAndName, newFolderPathAndName);

            try
            {
                foreach (var entry in context.SharedFolders.Where(x => x.CloudPathFromRoot.ToLower() == cloudPath.ToLower()))
                {
                    entry.Name = newName;

                    context.SharedFolders.Update(entry);
                }

                string oldPathAndName = Path.Combine(cloudPath, folderName);
                string newPathAndName = Path.Combine(cloudPath, newName);
                oldSharingPathAndName = Path.Combine(await ChangePathStructure(cloudPath), folderName);
                newSharingPathAndName = Path.Combine(await ChangePathStructure(cloudPath), newName);

                foreach (var entry in context.SharedFolders.Where(x => x.CloudPathFromRoot.ToLower().Contains(oldPathAndName.ToLower()))) //only at the beginning because of the "@" sign
                {
                    entry.CloudPathFromRoot = entry.CloudPathFromRoot.Replace(oldPathAndName, newPathAndName);

                    entry.SharedPathFromRoot = entry.SharedPathFromRoot.Replace(oldSharingPathAndName, newSharingPathAndName);

                    context.SharedFolders.Update(entry);
                }

                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                Directory.Move(newFolderPathAndName, folderPathAndName);

                throw new CloudFunctionStopException("error while renaming directory");
            }

            sharedData.UpdateCurrentPath(oldSharingPathAndName, newSharingPathAndName);

            return newName;
        }

        public async Task<string> RenameFile(string cloudPath, string fileName, string newName)
        {
            string newFileName = new string(newName);
            string filePath = ParseRootName(cloudPath);
            string filePathAndName = Path.Combine(filePath, fileName);
            string newFilePathAndName = Path.Combine(filePath, newFileName);

            if (!File.Exists(filePathAndName))
                throw new CloudFunctionStopException("file does not exist");

            if (File.Exists(newFilePathAndName) && fileName.ToLower() != newName.ToLower())
                RenameObject(filePath, ref newFileName, true);

            File.Move(filePathAndName, newFilePathAndName);

            try
            {
                foreach (var entry in context.SharedFiles.Where(x => x.CloudPathFromRoot.ToLower() == cloudPath.ToLower()))
                {
                    entry.Name = newFileName;

                    context.SharedFiles.Update(entry);

                }

                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                File.Move(newFilePathAndName, filePathAndName);

                throw new CloudFunctionStopException("error while renaming file");
            }

            return newFileName;
        }

        public async Task<string> CopyFile(string source, string destination, CloudUser user)
        {
            if (String.IsNullOrWhiteSpace(source))
            {
                throw new CloudFunctionStopException("invalid source of file");
            }

            if (String.IsNullOrWhiteSpace(destination))
            {
                throw new CloudFunctionStopException("invalid destination for copy");
            }

            try
            {
                string src = ParseRootName(source);
                string dest = ParseRootName(destination);

                FileInfo fi = new FileInfo(src);

                if (!fi.Exists)
                    throw new CloudFunctionStopException("source file does not exist");

                user = await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id) ?? throw new CloudFunctionStopException("user is not found"); //get current user state from database or show error

                if (user.UsedSpace + fi.Length > user.MaxSpace)
                    throw new CloudFunctionStopException("not enough storage");

                string name = new string(fi.Name);

                File.Copy(src, Path.Combine(dest, RenameObject(dest, ref name, true)));

                await UpdateUserStorageUsed(user, fi.Length);

                Pair<string, string> parentPathAndName = GetParentPathAndName(destination);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                await SetFileConnectedState(destination, name, ChangeOwnerIdentification(ChangeRootName(destination), user.UserName), user, connections.First, connections.Second);

                return name == fi.Name ? String.Empty : name;
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                throw new Exception("error while pasting file");
            }
        }

        public async Task<string> CopyFolder(string source, string destination, CloudUser user)
        {

            if (String.IsNullOrWhiteSpace(source))
            {
                throw new CloudFunctionStopException("invalid source of directory");
            }

            if (String.IsNullOrWhiteSpace(destination))
            {
                throw new CloudFunctionStopException("invalid destination for copy");
            }

            try
            {
                string src = ParseRootName(source);
                string dest = ParseRootName(destination);

                DirectoryInfo di = new DirectoryInfo(src);

                if (!di.Exists)
                    throw new CloudFunctionStopException("source directory does not exist");

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
                        if (!fi.Exists)
                            continue;

                        if (user.UsedSpace + fi.Length > user.MaxSpace)
                            throw new CloudFunctionStopException("user ran out of space, some actions may not finished")
;
                        File.Copy(fi.FullName, Path.Combine(newDirectoryPath, fi.Name));

                        user = await UpdateUserStorageUsed(user, fi.Length);
                    }

                    foreach (DirectoryInfo dir in directory.GetDirectories())
                    {
                        dirData.Enqueue(dir);
                    }
                }

                Pair<string, string> parentPathAndName = GetParentPathAndName(destination);

                Pair<bool, bool> connections = await FolderIsSharedInAppInWeb(parentPathAndName.First, parentPathAndName.Second);

                await SetObjectAndUnderlyingObjectsState(destination, name, ChangeOwnerIdentification(ChangeRootName(destination), user.UserName), user, connections.First, connections.Second);

                return name == di.Name ? String.Empty : name;
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                throw new Exception("Error while pasting directory");
            }
        }

        public async Task<CloudPathData> ChangeToDirectory(string path, CloudPathData pathData)
        {
            if (path.StartsWith(Constants.AbsolutePathMarker))
            {
                if (Directory.Exists(ParseRootName(path)))
                {
                    try
                    {
                        await pathData.SetPath(path, ParseRootName(Constants.PrivateRootName));

                        return pathData;
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        throw new CloudFunctionStopException(ex.Message);
                    }
                    catch (Exception)
                    {
                        throw new CloudFunctionStopException("unable to modify path");
                    }
                }

                throw new CloudFunctionStopException($"part of path does not exist");
            }
            else
            {
                if (path == String.Empty)
                {
                    return pathData;
                }

                string[] pathElements = path.Split(Path.DirectorySeparatorChar);

                foreach (string element in pathElements)
                {
                    if (element == String.Empty)
                    {
                        throw new CloudFunctionStopException($"part of path does not exist (empty path element)");
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
                                throw new CloudFunctionStopException($"part of path does not exist");
                            }
                        }
                        else
                        {
                            throw new CloudFunctionStopException($"part of path does not exist");
                        }
                    }
                }

                return pathData;
            }
        }

        public async Task<string> ListCurrentSubDirectories(CloudPathData pathData)
        {
            try
            {
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
            catch (Exception)
            {
                return Constants.TerminalRedText("error while printing items");
            }
        }

        public async Task<string> GetTerminalHelpText()
        {
            try
            {
                JArray commands = JArray.Parse(File.ReadAllText(Constants.TerminalCommandsDataFilePath));

                return await Task.FromResult<string>((String.Join('\n', commands?.Select(x => $"{Constants.TerminalYellowText(x[Constants.TerminalHelpName]?.ToString() ?? String.Empty)} : {x[Constants.TerminalHelpDescription]?.ToString() ?? String.Empty}") ?? Array.Empty<string>()) ?? Constants.TerminalRedText("no command data available")) + Environment.NewLine);

            }
            catch (Exception)
            {
                return Constants.TerminalRedText("error while loading help");
            }
        }

        public async Task<string> PrintWorkingDirectory(CloudPathData pathData)
        {
            return await Task.FromResult<string>(pathData.CurrentPathShow);
        }

        public async Task<List<CloudFolder>> SearchDirectoryInCurrentDirectory(string currentPath, string pattern)
        {
            try
            {
                return await GetCurrentDepthCloudDirectories(currentPath, pattern: pattern);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
        }

        public async Task<List<CloudFile>> SearchFileInCurrentDirectory(string currentPath, string pattern)
        {
            try
            {
                return await GetCurrentDepthCloudFiles(currentPath, pattern: pattern);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
        }

        public async Task CheckUserStorageUsed(CloudUser user)
        {
            user = await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id) ?? throw new CloudFunctionStopException("user is not found"); //get current user state from database or show error

            user.UsedSpace = await GetDirectorySize(Constants.PrivateRootName);

            context.Users.Update(user);

            await context.SaveChangesAsync();
        }

        public async Task<SharedFolder> GetWebSharedFolderPathById(Guid id)
        {
            return await context.SharedFolders.FirstOrDefaultAsync(x => x.Id == id) ?? throw new CloudFunctionStopException("folder not found");
        }

        public async Task<SharedFile> GetWebSharedFilePathById(Guid id)
        {
            return await context.SharedFiles.FirstOrDefaultAsync(x => x.Id == id) ?? throw new CloudFunctionStopException("folder not found");
        }

        #endregion

        #region Private Methods

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

            return Constants.SystemFolders.Select(x => x.ToLower()).Contains(pathFolders.Last().ToLower()) && ((pathFolders.Count - pathFolders.FindIndex(x => x == Constants.WebRootFolderName) - 1) == Constants.DistanceToRootFolder);
        }

        /// <summary>
        /// Method to make / remove / alter database entry
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="directoryName">Name of folder</param>
        /// <param name="sharingPath">Path in app (sharing)</param>
        /// <param name="user">Owner of folder</param>
        /// <param name="connectToApp">Boolean to connect folder to app</param>
        /// <param name="connectToWeb">Boolean to connect folder to web</param>
        /// <returns>Boolean value if action is successful</returns>
        /// <exception cref="CloudFunctionStopException">throw exception if given data invalid</exception>
        private async Task<bool> SetDirectoryConnectedState(string cloudPath, string directoryName, string sharingPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null, bool useSharingPath = false)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ParseRootName(cloudPath));

                DirectoryInfo? entryInfo = di.GetDirectories().FirstOrDefault(x => x.Name.ToLower() == directoryName.ToLower());

                if ((connectToApp == true || connectToWeb == true) && (entryInfo is null || !entryInfo.Exists))
                {
                    throw new CloudFunctionStopException("directory does not exist");
                }

                SharedFolder? sharedFolder = useSharingPath ? await context.SharedFolders.FirstOrDefaultAsync(x => x.SharedPathFromRoot.ToLower() == sharingPath.ToLower() && x.Name.ToLower() == directoryName.ToLower() && x.Owner == user) : await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot.ToLower() == cloudPath.ToLower() && x.Name.ToLower() == directoryName.ToLower() && x.Owner == user);

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
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to make / remove / alter database entry
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="sharingPath">Path in app (sharing)</param>
        /// <param name="user">Owner of file</param>
        /// <param name="connectToApp">Boolean to connect file to app</param>
        /// <param name="connectToWeb">Boolean to connect file to web</param>
        /// <returns>Boolean value if action is successful</returns>
        /// <exception cref="CloudFunctionStopException">throw exception if given data invalid</exception>
        private async Task<bool> SetFileConnectedState(string cloudPath, string fileName, string sharingPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null, bool useSharingPath = false)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ParseRootName(cloudPath));

                FileInfo? entryInfo = di.GetFiles().FirstOrDefault(x => x.Name.ToLower() == fileName.ToLower());

                if ((connectToApp == true || connectToWeb == true) && (entryInfo is null || !entryInfo.Exists))
                {
                    throw new CloudFunctionStopException("file does not exist");
                }

                SharedFile? sharedFile = useSharingPath ? await context.SharedFiles.FirstOrDefaultAsync(x => x.SharedPathFromRoot.ToLower() == sharingPath.ToLower() && x.Name.ToLower() == fileName.ToLower() && x.Owner == user) : await context.SharedFiles.FirstOrDefaultAsync(x => x.CloudPathFromRoot.ToLower() == cloudPath.ToLower() && x.Name.ToLower() == fileName.ToLower() && x.Owner == user);

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
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to set a create / remove / edit a folder and it's content (underlying tree) database entries
        /// </summary>
        /// <param name="cloudPath">Path in cloud</param>
        /// <param name="directoryName">Name of folder</param>
        /// <param name="sharingPath">Path in app (sharing)</param>
        /// <param name="user">Owner of folder</param>
        /// <param name="connectToApp">Boolean if folder connects to app</param>
        /// <param name="connectToWeb">Boolean if folder connects to web</param>
        /// <returns>Booelan value if action was successful</returns>
        /// <exception cref="CloudFunctionStopException">Throw is execution should be stopped</exception>
        private async Task<bool> SetObjectAndUnderlyingObjectsState(string cloudPath, string directoryName, string sharingPath, CloudUser user, bool? connectToApp = null, bool? connectToWeb = null, bool useSharingPath = false)
        {
            try
            {
                bool noError = true;

                if (useSharingPath == true && connectToApp == false)
                {
                    SharedFolder? sharedFolder = await context.SharedFolders.FirstOrDefaultAsync(x => x.SharedPathFromRoot.ToLower() == sharingPath.ToLower() && x.Name.ToLower() == directoryName.ToLower() && x.ConnectedToApp == true);

                    if (sharedFolder is not null)
                        cloudPath = sharedFolder.CloudPathFromRoot;
                }

                DirectoryInfo di = new DirectoryInfo(ParseRootName(cloudPath));

                DirectoryInfo? entryInfo = di.GetDirectories().FirstOrDefault(x => x.Name.ToLower() == directoryName.ToLower());

                if (entryInfo is null || !entryInfo.Exists)
                    throw new CloudFunctionStopException("part of path does not exist");

                Queue<Tuple<string, string, DirectoryInfo>> underlyingDirectories = new Queue<Tuple<string, string, DirectoryInfo>>(new List<Tuple<string, string, DirectoryInfo>>() { new Tuple<string, string, DirectoryInfo>(cloudPath, sharingPath, new DirectoryInfo(Path.Combine(ParseRootName(cloudPath), entryInfo?.Name ?? directoryName))) });

                while (underlyingDirectories.Any())
                {
                    var dir = underlyingDirectories.Dequeue();

                    try
                    {
                        noError = noError && await SetDirectoryConnectedState(dir.Item1, dir.Item3.Name, dir.Item2, user, connectToApp, connectToWeb, useSharingPath);
                    }
                    catch (Exception)
                    {
                        noError = false;
                    }

                    string cloudPathForItem = Path.Combine(dir.Item1, dir.Item3.Name);
                    string sharingPathForItem = Path.Combine(dir.Item2, dir.Item3.Name);

                    foreach (var subDirectory in dir.Item3.GetDirectories())
                    {
                        underlyingDirectories.Enqueue(new Tuple<string, string, DirectoryInfo>(cloudPathForItem, sharingPathForItem, subDirectory));

                        try
                        {
                            noError = noError && await SetDirectoryConnectedState(cloudPathForItem, subDirectory.Name, sharingPathForItem, user, connectToApp, connectToWeb, useSharingPath);
                        }
                        catch (Exception)
                        {
                            noError = false;
                        }
                    }

                    foreach (var subFile in dir.Item3.GetFiles())
                    {
                        try
                        {
                            noError = noError && await SetFileConnectedState(cloudPathForItem, subFile.Name, sharingPathForItem, user, connectToApp, connectToWeb, useSharingPath);
                        }
                        catch (Exception)
                        {
                            noError = false;
                        }
                    }
                }

                return noError;
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Method to get from a specified folder if that is shared in web
        /// </summary>
        /// <param name="cloudPath">Path in app</param>
        /// <param name="folderName">Name of folder</param>
        /// <returns>Boolean value indicating the sharing of folder</returns>
        private async Task<bool> FolderIsSharedInWeb(string cloudPath, string folderName)
        {
            return await context.SharedFolders.FirstOrDefaultAsync(x => x.CloudPathFromRoot == cloudPath && x.Name == folderName && x.ConnectedToWeb) != null;
        }

        /// <summary>
        /// Private method to get a folder size in bytes
        /// </summary>
        /// <param name="cloudPath">Path to folder in app</param>
        /// <returns></returns>
        /// <exception cref="CloudFunctionStopException">Throws if source folder does not exist</exception>
        /// <exception cref="Exception">Throws in unexpected error happened</exception>
        private async Task<double> GetDirectorySize(string cloudPath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(ParseRootName(cloudPath));

                if (!di.Exists)
                    throw new CloudFunctionStopException("source directory does not exist");

                double size = 0.0;

                foreach (FileInfo file in di.GetFiles("*.*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }

                return await Task.FromResult<double>(size);
            }
            catch (CloudFunctionStopException ex)
            {
                throw new CloudFunctionStopException(ex.Message);
            }
            catch (Exception)
            {
                throw new Exception("Error while getting directory size");
            }
        }

        /// <summary>
        /// Private method to update storage occupied by user
        /// </summary>
        /// <param name="user">Currently logged in user</param>
        /// <param name="amountToBeAdded">The storage to be added/deducted</param>
        /// <returns>The modified user</returns>
        /// <exception cref="CloudFunctionStopException">Throws if user is not found</exception>
        private async Task<CloudUser> UpdateUserStorageUsed(CloudUser user, double amountToBeAdded)
        {

            user = await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id) ?? throw new CloudFunctionStopException("user is not found"); //get current user state from database or show error

            user.UsedSpace += amountToBeAdded; //updating user used space

            if (user.UsedSpace < 0)
                user.UsedSpace = 0;

            if (user.UsedSpace > Constants.UserSpaceSize)
                user.UsedSpace = Constants.UserSpaceSize;

            context.Users.Update(user);

            await context.SaveChangesAsync();

            return user;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Method to rename a file or folder to a non-existent in the actual folder
        /// </summary>
        /// <param name="physicalPath">path to the folder or file (on disk)</param>
        /// <param name="name">name of file, folder (due to ref keyword, it will be modified)</param>
        /// <param name="isFile">Boolean value if passed data refer to file</param>
        /// <returns>The modified name</returns>
        private static string RenameObject(string physicalPath, ref string name, bool isFile)
        {
            int counter = 0;

            string pathAndName = Path.Combine(physicalPath, name);

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

                    pathAndName = Path.Combine(physicalPath, name);
                }
            }
            else
            {
                string nameBase = new string(name);

                while (System.IO.Directory.Exists(pathAndName))
                {
                    name = nameBase + Constants.FileNameDelimiter + (++counter).ToString();

                    pathAndName = Path.Combine(physicalPath, name);
                }
            }

            return name;
        }

        /// <summary>
        /// Static method to change the owner identification in a path (from app)
        /// </summary>
        /// <param name="path">cloud or shared path</param>
        /// <param name="itemForChange">Item to replace the identifiers</param>
        /// <returns>The modified path as a string</returns>
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

        /// <summary>
        /// Static method to get owner identification from shared path
        /// </summary>
        /// <param name="sharedPath">Path to be parsed</param>
        /// <returns>The owner identifier as a string (shared path has username)</returns>
        private static string GetSharedPathOwnerUser(string sharedPath)
        {
            try
            {
                return sharedPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)[1];
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

        /// <summary>
        /// Private static method to get a string length in bytes
        /// </summary>
        /// <param name="content">The content to measure</param>
        /// <returns>The size in bytes converted to double</returns>
        private static double GetStringLengthInBytes(string content)
        {
            return Encoding.UTF8.GetBytes(content).Length;
        }
        #endregion
    }
}
