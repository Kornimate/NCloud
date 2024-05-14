using NCloud.Models;
using NCloud.Users;
using System.Security.Claims;

namespace NCloud.Services
{
    public interface ICloudService
    {
        /// <summary>
        /// Method to get current state files
        /// </summary>
        /// <param name="cloudPath">The actual path in the cloud</param>
        /// <param name="connectToApp">Parameter if listed folders are only connected to app</param>
        /// <param name="connectToWeb">Parameter if listed folders are only connected to web/param>
        /// <param name="pattern">Pattern to get only folders matching the given pattern</param>
        /// <returns></returns>
        Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string cloudPath, bool connectToApp = false, bool connectToWeb = false, string? pattern = null);

        /// <summary>
        /// Method to get current state files
        /// </summary>
        /// <param name="cloudPath">The actual path in the cloud</param>
        /// <param name="connectToApp">Parameter if listed files are only connected to app</param>
        /// <param name="connectToWeb">Parameter if listed files are only connected to web/param>
        /// <param name="pattern">Pattern to get only files matching the given pattern</param>
        /// <returns></returns>
        Task<List<CloudFile>> GetCurrentDepthCloudFiles(string cloudPath, bool connectToApp = false, bool connectToWeb = false, string? pattern = null);
        Task<CloudUser?> GetAdmin();
        Task<bool> CreateBaseDirectoryForUser(CloudUser cloudUser);
        Task<string> CreateDirectory(string folderName, string currentPath, CloudUser user);
        Task<string> CreateFile(IFormFile file, string currentPath, ClaimsPrincipal userPrincipal);
        Task<bool> RemoveDirectory(string folderName, string currentPath, ClaimsPrincipal userPrincipal);
        Task<bool> RemoveFile(string fileName, string currentPath, ClaimsPrincipal userPrincipal);
        string ServerPath(string currentPath);
        string ChangeRootName(string currentPath);
        Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData();
        bool DirectoryExists(string? pathAndName);
        Task<DirectoryInfo> GetFolderByPath(string path, string folderName);
        Task<bool> ConnectDirectoryToWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> ConnectDirectoryToApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> DisconnectDirectoryFromApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> DisconnectDirectoryFromWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> ConnectFileToApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<bool> ConnectFileToWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<bool> DisconnectFileFromApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<bool> DisconnectFileFromWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<List<CloudFolder>> GetSharingUsersSharingDirectories(string currentPath);
        Task<List<CloudFile>> GetCurrentDepthAppSharingFiles(string currentPath);
        Task<List<CloudFolder>> GetCurrentDepthAppSharingDirectories(string currentPath);
        Task<bool> OwnerOfPathIsActualUser(string currentPath, ClaimsPrincipal userPrincipal);
        Task<List<string>> GetUserSharedFolderUrls(ClaimsPrincipal userPrincipal);
        Task<List<string>> GetUserSharedFileUrls(ClaimsPrincipal userPrincipal);
        Task<string> WebBackCheck(string path);
        Task<List<CloudFile>> GetCurrentDepthWebSharingFiles(string path);
        Task<List<CloudFolder>> GetCurrentDepthWebSharingDirectories(string path);
        Task<string?> CreateZipFile(List<string> itemsForDownload, string currentPath, string filePath, bool connectedToApp, bool connectedToWeb);
        Task<string> ChangePathStructure(string currentPath);
        Task<bool> ModifyFileContent(string file, string content);
        Task<CloudFolder> GetFolder(string currentPath, string folderName);
        Task<CloudFile> GetFile(string currentPath, string fileName);
        Task<string> RenameFolder(string currentPath, string folderName, string newName);
        Task<string> RenameFile(string currentPath, string fileName, string newName);
        Task<string> CopyFile(string? source, string destination, ClaimsPrincipal userPrincipal);
        Task<string> CopyFolder(string? source, string destination, ClaimsPrincipal userPrincipal);
        Task<CloudPathData> GetSessionCloudPathData();
        Task<bool> SetSessionCloudPathData(CloudPathData pathData);
        Task<string> ChangeToDirectory(string path);
        Task<string> ListCurrentSubDirectories();
        Task<string> GetTerminalHelpText();
        Task<string> PrintWorkingDirectory();
        Task<List<CloudFolder>> SearchDirectoryInCurrentDirectory(string currentPath, string pattern);
        Task<List<CloudFile>> SearchFileInCurrentDirectory(string currentPath, string pattern);
    }
}
