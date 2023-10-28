using NCloud.Models;
using NCloud.Users;

namespace NCloud.Services
{
    public interface ICloudService
    {
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath);
        public List<CloudFile?> GetCurrentDeptFiles(string currentPath);
        CloudUser GetAdmin();
        bool CreateDirectory(string folderName, string currentPath);
        Task<int> CreateFile(IFormFile file, string currentPath);
        bool RemoveDirectory(string folderName, string currentPath);
        bool RemoveFile(string fileName, string currentPath);
        string ReturnServerPath(string currentPath);
        Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData();
    }
}
