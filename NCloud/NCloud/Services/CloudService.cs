using NCloud.Models;
using FileII = NCloud.Models.FileII;
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

        public CloudUser GetAdmin()
        {
            return context.Users.FirstOrDefault(x => x.UserName== "Admin")!;
        }

        public List<CloudFile?> GetCurrentDeptFiles(string currentPath)
        {
            return Directory.GetFiles(currentPath.Replace("@CLOUDROOT", Path.Combine(env.WebRootPath,"CloudData", "UserData"))).Select(x => new CloudFile(new FileInfo(x),icon:x)).ToList()!;
        }
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath)
        {
            return Directory.GetDirectories(currentPath.Replace("@CLOUDROOT", Path.Combine(env.WebRootPath,"CloudData", "UserData"))).Select(x => new CloudFolder(new DirectoryInfo(x))).ToList()!;
        }

        public Tuple<List<FileII?>, List<FolderII?>> GetCurrentUserIndexData()
        {
            return new(null!,null!);
        }
    }
}
