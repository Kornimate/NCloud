using NCloud.Models;
using FileII = NCloud.Models.FileII;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private const string ROOTNAME = "@CLOUDROOT";
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;

        public CloudService(CloudDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        public bool CreateDirectory(string folderName, string currentPath)
        {
            string pathAndName = Path.Combine(ParseRootName(currentPath), folderName);
            if (Directory.Exists(pathAndName))
            {
                throw new Exception("The Folder already exists!");
            }
            Directory.CreateDirectory(pathAndName);
            return true;
        }

        public CloudUser GetAdmin()
        {
            return context.Users.FirstOrDefault(x => x.UserName== "Admin")!;
        }

        public List<CloudFile?> GetCurrentDeptFiles(string currentPath)
        {
            return Directory.GetFiles(ParseRootName(currentPath)).Select(x => new CloudFile(new FileInfo(x),icon:x)).ToList()!;
        }
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath)
        {
            return Directory.GetDirectories(ParseRootName(currentPath)).Select(x => new CloudFolder(new DirectoryInfo(x))).ToList()!;
        }

        public Tuple<List<FileII?>, List<FolderII?>> GetCurrentUserIndexData()
        {
            return new(null!,null!);
        }

        private string ParseRootName(string currentPath)
        {
            return currentPath.Replace(ROOTNAME, Path.Combine(env.WebRootPath, "CloudData", "UserData"));
        }
    }
}
