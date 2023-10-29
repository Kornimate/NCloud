using NCloud.Models;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using Castle.Core.Internal;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Identity;

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
        public bool CreateBaseDirectory(CloudUser cloudUser)
        {
            string userFolderPath = Path.Combine(env.WebRootPath, "CloudData", "UserData", cloudUser.Id);
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
                CreateJsonConatinerFile(userFolderPath);
            }
            string sharedFolderPath = Path.Combine(env.WebRootPath, "CloudData", "Public");
            if (!Directory.Exists(sharedFolderPath))
            {
                Directory.CreateDirectory(sharedFolderPath);
                CreateJsonConatinerFile(sharedFolderPath);
            }
            string pathHelper = Path.Combine(env.WebRootPath, "CloudData", "Public", cloudUser.UserName);
            if (!Directory.Exists(pathHelper))
            {
                Directory.CreateDirectory(pathHelper);
                CreateJsonConatinerFile(pathHelper);
                AddFolderToJsonContainerFile(sharedFolderPath, cloudUser.UserName, cloudUser.UserName);
            }
            List<string> baseFolders = new List<string>() { "Documents", "Pictures", "Videos", "Music" };
            foreach (string folder in baseFolders)
            {
                pathHelper = Path.Combine(userFolderPath, folder);
                Directory.CreateDirectory(pathHelper);
                CreateJsonConatinerFile(pathHelper);
                AddFolderToJsonContainerFile(userFolderPath, folder, cloudUser.UserName);
            }
            return true;
        }

        public bool CreateDirectory(string folderName, string currentPath, string owner)
        {
            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, folderName);
            if (Directory.Exists(pathAndName))
            {
                throw new Exception("The Folder already exists!");
            }
            Directory.CreateDirectory(pathAndName);
            CreateJsonConatinerFile(pathAndName);
            AddFolderToJsonContainerFile(path, folderName, owner);
            return true;
        }
        private void CreateJsonConatinerFile(string? path)
        {
            if (path is null) return;
            JsonDataContainer container = new JsonDataContainer()
            {
                FolderName = path
            };
            System.IO.File.WriteAllText(Path.Combine(path, JSONCONTAINERNAME), JsonSerializer.Serialize<JsonDataContainer>(container));
        }

        private void AddFolderToJsonContainerFile(string? path, string? folderName, string owner)
        {
            if (path is null) return;
            if (folderName is null) return;
            JsonDataContainer container = JsonSerializer.Deserialize<JsonDataContainer>(System.IO.File.ReadAllText(Path.Combine(path, JSONCONTAINERNAME)))!;
            container.Folders?.Add(folderName, new JsonDetailsContainer
            {
                Owner = owner,
                IsShared = false
            });
            System.IO.File.WriteAllText(Path.Combine(path, JSONCONTAINERNAME), JsonSerializer.Serialize<JsonDataContainer>(container));
        }

        public async Task<int> CreateFile(IFormFile file, string currentPath, string owner)
        {
            int retNum = 1;
            try
            {
                string path = ParseRootName(currentPath);
                string pathAndName = Path.Combine(path, file.FileName);
                string newName = file.FileName;
                int counter = 0;
                while (System.IO.File.Exists(pathAndName))
                {
                    FileInfo fi = new FileInfo(file.FileName);
                    newName = fi.Name.Split('.')[0] + FILENAMEDELIMITER + $"{++counter}" + fi.Extension;
                    pathAndName = Path.Combine(path, newName);
                    retNum = 0;
                }
                using (FileStream stream = new FileStream(pathAndName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                AddFileToJsonContainerFile(path, newName, owner);
            }
            catch
            {
                retNum = -1; //hibalépett fel
            }
            return retNum;
        }

        private void AddFileToJsonContainerFile(string? path, string? fileName, string owner)
        {
            if (path is null) return;
            if (fileName is null) return;
            JsonDataContainer container = JsonSerializer.Deserialize<JsonDataContainer>(System.IO.File.ReadAllText(Path.Combine(path, JSONCONTAINERNAME)))!;
            container.Files?.Add(fileName, new JsonDetailsContainer
            {
                Owner = owner,
                IsShared = false
            });
            System.IO.File.WriteAllText(Path.Combine(path, JSONCONTAINERNAME), JsonSerializer.Serialize<JsonDataContainer>(container));
        }

        public CloudUser GetAdmin()
        {
            return context.Users.FirstOrDefault(x => x.UserName == "Admin")!;
        }

        public List<CloudFile?> GetCurrentDeptFiles(string currentPath)
        {
            string path = ParseRootName(currentPath);
            JsonDataContainer container = JsonSerializer.Deserialize<JsonDataContainer>(System.IO.File.ReadAllText(Path.Combine(path, JSONCONTAINERNAME)))!;
            return container.Files!.Select(x => new CloudFile(new FileInfo(Path.Combine(path, x.Key)), x.Value.Owner, x.Value.IsShared, x.Key)).ToList()!;
        }
        public List<CloudFolder?> GetCurrentDeptFolders(string currentPath)
        {
            string path = ParseRootName(currentPath);
            JsonDataContainer container = JsonSerializer.Deserialize<JsonDataContainer>(System.IO.File.ReadAllText(Path.Combine(path, JSONCONTAINERNAME)))!;
            return container.Folders!.Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(path, x.Key)), x.Value.Owner, x.Value.IsShared)).ToList()!;
        }

        public bool RemoveDirectory(string folderName, string currentPath)
        {
            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, folderName);
            if (!Directory.Exists(pathAndName))
            {
                throw new Exception("The Folder does not exists!");
            }
            if (IsSystemFolder(pathAndName))
            {
                return false;
            }
            Directory.Delete(pathAndName, true);
            RemoveFolderFromJsonContainerFile(path, folderName);
            return true;
        }

        private void RemoveFolderFromJsonContainerFile(string? path, string? folderName)
        {
            if (path is null) return;
            if (folderName is null) return;
            JsonDataContainer container = JsonSerializer.Deserialize<JsonDataContainer>(System.IO.File.ReadAllText(Path.Combine(path, JSONCONTAINERNAME)))!;
            container.Folders?.Remove(folderName);
            System.IO.File.WriteAllText(Path.Combine(path, JSONCONTAINERNAME), JsonSerializer.Serialize<JsonDataContainer>(container));
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
            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, fileName);
            if (!File.Exists(pathAndName))
            {
                throw new Exception("The File does not exists!");
            }
            File.Delete(pathAndName);
            RemoveFileFromJsonContainerFile(path, fileName);
            return true;
        }
        private void RemoveFileFromJsonContainerFile(string? path, string? fileName)
        {
            if (path is null) return;
            if (fileName is null) return;
            JsonDataContainer container = JsonSerializer.Deserialize<JsonDataContainer>(System.IO.File.ReadAllText(Path.Combine(path, JSONCONTAINERNAME)))!;
            container.Files?.Remove(fileName);
            System.IO.File.WriteAllText(Path.Combine(path, JSONCONTAINERNAME), JsonSerializer.Serialize<JsonDataContainer>(container));
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
