using NCloud.Models;
using NCloud.Users;

namespace NCloud.Services
{
    public interface ICloudService
    {
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath);
        public List<CloudFile?> GetCurrentDeptFiles(string currentPath);
        CloudUser GetAdmin();
        bool CreateBaseDirectory(CloudUser cloudUser);
        void CreateDirectory(string folderName, string currentPath, string owner);
        Task<int> CreateFile(IFormFile file, string currentPath, string owner);
        bool RemoveDirectory(string folderName, string currentPath);
        bool RemoveFile(string fileName, string currentPath);
        string ReturnServerPath(string currentPath);
        Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData();
        bool DirectoryExists(string? pathAndName);
    }
}
