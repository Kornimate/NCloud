using NCloud.Models;
using File = NCloud.Models.File;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        public Tuple<List<File>,List<Folder>> GetCurrentUserIndexData()
        {
            throw new NotImplementedException();
        }
    }
}
