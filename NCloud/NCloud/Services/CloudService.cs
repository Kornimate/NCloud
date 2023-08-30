using NCloud.Models;
using File = NCloud.Models.File;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;

        public CloudService(CloudDbContext context,IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        public List<CloudRegistration?> GetCurrentDeptData(int parentId, CloudUser user)
        {
            return context.Entries.Where(x => x.ParentId == parentId)
                                  .Select(x => CloudRegistration.CreateRegistration(x))
                                  .ToList();
        }

        public Tuple<List<File?>, List<Folder?>> GetCurrentUserIndexData()
        {
            return new
                (
                     context.Entries.Where(x => x.Type == EntryType.FILE)
                                    .OrderByDescending(x => x.CreatedDate)
                                    .Take(5)
                                    .Select(x => (File?)CloudRegistration.CreateRegistration(x))
                                    .ToList(),
                     context.Entries.Where(x => x.Type == EntryType.FILE)
                                    .OrderByDescending(x => x.CreatedDate)
                                    .Take(5)
                                    .Select(x => (Folder?)CloudRegistration.CreateRegistration(x))
                                    .ToList()
                );
        }
    }
}
