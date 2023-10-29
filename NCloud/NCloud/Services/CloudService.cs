using NCloud.Models;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using Castle.Core.Internal;
using System.Drawing.Drawing2D;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private const string ROOTNAME = "@CLOUDROOT";
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;
        private const int DISTANCE = 4;
        private readonly List<string> systemFolders;
        private const string FILENAMEDELIMITER = "_";
        private const string JSONCONTAINERNAME = "__JsonContainer__.json";

        public CloudService(CloudDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
            systemFolders = new List<string>()
            {
                "Music",
                "Documents",
                "Videos",
                "Pictures"
            };
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

        public async Task<int> CreateFile(IFormFile file, string currentPath)
        {
            int retNum = 1;
            try
            {
                string pathAndName = Path.Combine(ParseRootName(currentPath), file.FileName);
                int counter = 0;
                while (System.IO.File.Exists(pathAndName))
                {
                    FileInfo fi = new FileInfo(file.FileName);
                    string newName = fi.Name.Split('.')[0] + FILENAMEDELIMITER + $"{++counter}" + fi.Extension;
                    pathAndName = Path.Combine(ParseRootName(currentPath), newName);
                    retNum = 0;
                }
                using (FileStream stream = new FileStream(pathAndName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch
            {
                retNum = -1; //hibalépett fel
            }
            return retNum;
        }

        public CloudUser GetAdmin()
        {
            return context.Users.FirstOrDefault(x => x.UserName == "Admin")!;
        }

        public List<CloudFile?> GetCurrentDeptFiles(string currentPath)
        {
            return Directory.GetFiles(ParseRootName(currentPath)).Where(x => !x.Contains(JSONCONTAINERNAME)).Select(x => new CloudFile(new FileInfo(x), icon: x)).ToList()!;
        }
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath)
        {
            return Directory.GetDirectories(ParseRootName(currentPath)).Select(x => new CloudFolder(new DirectoryInfo(x))).ToList()!;
        }

        public bool RemoveDirectory(string folderName, string currentPath)
        {
            string pathAndName = Path.Combine(ParseRootName(currentPath), folderName);
            if (!Directory.Exists(pathAndName))
            {
                throw new Exception("The Folder does not exists!");
            }
            if (IsSystemFolder(pathAndName))
            {
                return false;
            }
            Directory.Delete(pathAndName, true);
            return true;
        }

        private string ParseRootName(string currentPath)
        {
            return currentPath.Replace(ROOTNAME, Path.Combine(env.WebRootPath, "CloudData", "UserData"));
        }

        private bool IsSystemFolder(string path)
        {
            List<string> pathFolders = path.Split('\\').ToList();
            return systemFolders.Contains(pathFolders[pathFolders.FindIndex(x => x == "wwwroot") + DISTANCE]);
        }

        public bool RemoveFile(string fileName, string currentPath)
        {
            string pathAndName = Path.Combine(ParseRootName(currentPath), fileName);
            if (!File.Exists(pathAndName))
            {
                throw new Exception("The File does not exists!");
            }
            File.Delete(pathAndName);
            return true;
        }

        public string ReturnServerPath(string currentPath)
        {
            return ParseRootName(currentPath);
        }

        public Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData()
        {
            return new Tuple<List<CloudFile?>, List<CloudFolder?>>(new(), new());
        }
    }
}
