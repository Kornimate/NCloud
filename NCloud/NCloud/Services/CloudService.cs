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

        public CloudService(CloudDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        public List<FileInfo?> GetCurrentDeptData(string currentPath)
        {
            return Directory.GetFiles(currentPath.Replace("@CLOUDROOT", Path.Combine(env.WebRootPath, "UserData"))).Select(x => new FileInfo(x)).ToList()!;
        }

        public Tuple<List<File?>, List<Folder?>> GetCurrentUserIndexData()
        {
            return new(null!,null!);
        }
    }
}
