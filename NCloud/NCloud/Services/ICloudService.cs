using NCloud.Models;
using NCloud.Users;
using FileII = NCloud.Models.FileII;

namespace NCloud.Services
{
    public interface ICloudService
    {
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath);
        public List<CloudFile?> GetCurrentDeptFiles(string currentPath);
        Tuple<List<FileII?>, List<FolderII?>> GetCurrentUserIndexData();
    }
}
