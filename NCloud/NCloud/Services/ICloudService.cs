using NCloud.Models;
using NCloud.Users;
using File = NCloud.Models.File;

namespace NCloud.Services
{
    public interface ICloudService
    {
        List<FileInfo?> GetCurrentDeptData(string currentPath);
        Tuple<List<File?>, List<Folder?>> GetCurrentUserIndexData();
    }
}
