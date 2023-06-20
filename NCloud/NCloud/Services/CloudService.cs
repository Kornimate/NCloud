using NCloud.Models;
using File = NCloud.Models.File;
using Microsoft.EntityFrameworkCore;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;

        public CloudService(CloudDbContext context)
        {
            this.context = context;
        }

        public Tuple<List<File?>,List<Folder?>> GetCurrentUserIndexData()
        {
            return new
                (
                     context.Entries.Where(x=>x.Type == EntryType.FILE)
                                    .OrderByDescending(x=>x.CreatedDate)
                                    .Take(5)
                                    .Select(x=>x as File)
                                    .ToList(),
                     context.Entries.Where(x => x.Type == EntryType.FILE)
                                    .OrderByDescending(x => x.CreatedDate)
                                    .Take(5)
                                    .Select(x => x as Folder)
                                    .ToList()
                );
        }
    }
}
