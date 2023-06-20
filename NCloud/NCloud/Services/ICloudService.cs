using NCloud.Models;
using NCloud.Users;
using File = NCloud.Models.File;

namespace NCloud.Services
{
    public interface ICloudService
    {
        List<CloudRegistration?> GetCurrentDeptData(int parentId,CloudUser user);
        Tuple<List<File?>, List<Folder?>> GetCurrentUserIndexData();
    }
}
