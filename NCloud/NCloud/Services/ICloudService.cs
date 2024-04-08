using NCloud.Models;
using NCloud.Users;
using System.Security.Claims;

namespace NCloud.Services
{
    public interface ICloudService
    {
        Task<List<CloudFolder>> GetCurrentDepthDirectories(string currentPath);
        Task<List<CloudFile>> GetCurrentDepthFiles(string currentPath);
        Task<CloudUser?> GetAdmin();
        Task<bool> CreateBaseDirectory(CloudUser cloudUser);
        Task CreateDirectory(string folderName, string currentPath);
        Task<int> CreateFile(IFormFile file, string currentPath);
        Task<bool> RemoveDirectory(string folderName, string currentPath);
        Task<bool> RemoveFile(string fileName, string currentPath);
        string ServerPath(string currentPath);
        Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData();
        bool DirectoryExists(string? pathAndName);
        Task<DirectoryInfo> GetFolderByPath(string path, string folderName);
        Task<bool> ConnectDirectoryToWeb(string currentPath, string directoryName, ClaimsPrincipal userPrincipal);
    }
}
