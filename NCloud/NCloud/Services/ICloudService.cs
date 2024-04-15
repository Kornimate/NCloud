using NCloud.Models;
using NCloud.Users;
using System.Security.Claims;

namespace NCloud.Services
{
    public interface ICloudService
    {
        Task<List<CloudFolder>> GetCurrentDepthCloudDirectories(string currentPath, ClaimsPrincipal userPrincipal);
        Task<List<CloudFile>> GetCurrentDepthCloudFiles(string currentPath, ClaimsPrincipal userPrincipal);
        Task<CloudUser?> GetAdmin();
        Task<bool> CreateBaseDirectory(CloudUser cloudUser);
        Task CreateDirectory(string folderName, string currentPath, ClaimsPrincipal userPrincipal);
        Task<int> CreateFile(IFormFile file, string currentPath, ClaimsPrincipal userPrincipal);
        Task<bool> RemoveDirectory(string folderName, string currentPath, ClaimsPrincipal userPrincipal);
        Task<bool> RemoveFile(string fileName, string currentPath, ClaimsPrincipal userPrincipal);
        string ServerPath(string currentPath);
        Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData();
        bool DirectoryExists(string? pathAndName);
        Task<DirectoryInfo> GetFolderByPath(string path, string folderName);
        Task<bool> ConnectDirectoryToWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> ConnectDirectoryToApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> DisonnectDirectoryFromApp(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> DisconnectDirectoryFromWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
        Task<bool> ConnectFileToApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<bool> ConnectFileToWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<bool> DisonnectFileFromApp(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<bool> DisonnectFileFromWeb(string currentPath, string fileName, ClaimsPrincipal userPrincipal);
        Task<List<CloudFolder>> GetSharingUsersSharingDirectories(string currentPath);
        Task<List<CloudFile>> GetCurrentDepthSharingFiles(string currentPath, ClaimsPrincipal userPrincipal);
        Task<List<CloudFolder>> GetCurrentDepthSharingDirectories(string currentPath, ClaimsPrincipal userPrincipal);
        Task<bool> OwnerOfPathIsActualUser(string currentPath, ClaimsPrincipal userPrincipal);
        Task<List<string>> GetUserSharedFolderUrls(ClaimsPrincipal userPrincipal);
        Task<List<string>> GetUserSharedFileUrls(ClaimsPrincipal userPrincipal);
    }
}
