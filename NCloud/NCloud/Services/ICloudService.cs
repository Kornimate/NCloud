using NCloud.Models;
using File = NCloud.Models.File;

namespace NCloud.Services
{
    public interface ICloudService
    {
        Tuple<List<File?>, List<Folder?>> GetCurrentUserIndexData();
    }
}
