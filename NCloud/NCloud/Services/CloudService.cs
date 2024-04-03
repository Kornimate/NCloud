using NCloud.Models;
using Microsoft.EntityFrameworkCore;
using NCloud.Users;
using Castle.Core.Internal;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.IO;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using static NuGet.Packaging.PackagingConstants;

namespace NCloud.Services
{
    public class CloudService : ICloudService
    {
        private const string USERROOTNAME = "@CLOUDROOT";
        private const string SHAREDROOTNAME = "@SHAREDROOT";
        private readonly CloudDbContext context;
        private readonly IWebHostEnvironment env;
        private const int DISTANCE = 4;
        private readonly List<string> systemFolders;
        private const string FILENAMEDELIMITER = "_";
        private const string JSONCONTAINERNAME = "__JsonContainer__.json";
        private readonly TimeSpan MAXWAITTIME = TimeSpan.FromSeconds(10);

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
            string userFolderPath = Path.Combine(env.WebRootPath, "CloudData", "UserData", cloudUser.Id.ToString());
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
                CreateJsonContainerFile(userFolderPath);
            }
            string sharedFolderPath = Path.Combine(env.WebRootPath, "CloudData", "Public");
            if (!Directory.Exists(sharedFolderPath))
            {
                Directory.CreateDirectory(sharedFolderPath);
                CreateJsonContainerFile(sharedFolderPath);
            }
            string pathHelper = Path.Combine(env.WebRootPath, "CloudData", "Public", cloudUser.UserName);
            if (!Directory.Exists(pathHelper))
            {
                Directory.CreateDirectory(pathHelper);
                CreateJsonContainerFile(pathHelper);
                AddFolderToJsonContainerFile(sharedFolderPath, cloudUser.UserName, cloudUser.UserName);
            }
            List<string> baseFolders = new List<string>() { "Documents", "Pictures", "Videos", "Music" };
            foreach (string folder in baseFolders)
            {
                try
                {
                    pathHelper = Path.Combine(userFolderPath, folder);
                    Directory.CreateDirectory(pathHelper);
                    CreateJsonContainerFile(pathHelper);
                    AddFolderToJsonContainerFile(userFolderPath, folder, cloudUser.UserName);
                }
                catch
                {
                    return false; //TODO: handle this false in method that called this method
                }
            }
            return true;
        }

        public void CreateDirectory(string folderName, string currentPath, string owner)
        {
            if (folderName == null || folderName == String.Empty)
            {
                throw new Exception("Invalid Folder Name!");
            }

            if (currentPath == null || currentPath == String.Empty)
            {
                throw new Exception("Invalid Path!");
            }

            if (owner == null || owner == String.Empty)
            {
                throw new Exception("Invalid Owner!");
            }

            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, folderName);
            try
            {
                AddFolderToJsonContainerFile(path, folderName, owner);
                CreateJsonContainerFile(pathAndName);
            }
            catch
            {
                throw;
            }
        }
        private void CreateJsonContainerFile(string? path)
        {
            if (path is null) return;
            string pathAndName = Path.Combine(path, JSONCONTAINERNAME);
            JsonDataContainer container = new JsonDataContainer()
            {
                FolderName = path,
                StatusOK = true
            };
            if (File.Exists(pathAndName)) return; // just for safety reasons, not to overwrite existing files
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            System.IO.File.WriteAllText(pathAndName, JsonSerializer.Serialize<JsonDataContainer>(container));
        }

        private void AddFolderToJsonContainerFile(string? path, string? folderName, string owner)
        {
            try
            {
                Task task = Task.Run(async () =>
                {
                    try
                    {
                        using FileStream fs = File.Open(Path.Combine(path!, JSONCONTAINERNAME), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        JsonDataContainer? container = await JsonSerializer.DeserializeAsync<JsonDataContainer>(fs) ?? throw new Exception("Error while importing Folder Data!");
                        if (container.Folders!.ContainsKey(folderName!))
                        {
                            throw new ArgumentException("The Folder already exists!");
                        }
                        container.Folders?.Add(folderName!, new JsonDetailsContainer
                        {
                            Owner = owner,
                            IsShared = false
                        });
                        fs.SetLength(0);
                        await JsonSerializer.SerializeAsync(fs, container);
                    }
                    catch (IOException)
                    {
                        ; // keep trying until 10 seconds
                    }
                    catch
                    {
                        throw;
                    }
                });
                if (!task.Wait(MAXWAITTIME))
                {
                    throw new TimeoutException("Unable to manage Container File!");
                }
            }
            catch (Exception ex) when (ex.InnerException is ArgumentException || ex.InnerException is TimeoutException)
            {
                throw;
            }
            catch
            {
                throw new Exception("Error occurred while adding Folder!");
            }
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
                AddFileToJsonContainerFile(path, newName, owner);
                using (FileStream stream = new FileStream(pathAndName, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch
            {
                retNum = -1; //error occurred
            }
            return retNum;
        }

        private void AddFileToJsonContainerFile(string? path, string? fileName, string owner)
        {
            try
            {
                Task task = Task.Run(async () =>
                {
                    try
                    {
                        using FileStream fs = File.Open(Path.Combine(path!, JSONCONTAINERNAME), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        JsonDataContainer? container = await JsonSerializer.DeserializeAsync<JsonDataContainer>(fs) ?? throw new Exception("Error while importing File Data!");
                        if (container.Files!.ContainsKey(fileName!)) //just for safety, but controller ensures no same file name
                        {
                            throw new ArgumentException("The File already exists!");
                        }
                        container.Files?.Add(fileName!, new JsonDetailsContainer
                        {
                            Owner = owner,
                            IsShared = false
                        });
                        fs.SetLength(0);
                        await JsonSerializer.SerializeAsync(fs, container);
                    }
                    catch (IOException)
                    {
                        ; // keep trying until 10 seconds
                    }
                    catch
                    {
                        throw;
                    }
                });
                if (!task.Wait(MAXWAITTIME))
                {
                    throw new TimeoutException("Unable to manage Container File!");
                }
            }
            catch (Exception ex) when (ex.InnerException is ArgumentException || ex.InnerException is TimeoutException)
            {
                throw;
            }
            catch
            {
                throw new Exception("Error occurred while adding Folder!");
            }
        }

        public async Task<CloudUser?> GetAdmin()
        {
            return await context.Users.FirstOrDefaultAsync(x => x.UserName == "Admin");
        }

        public List<CloudFile?> GetCurrentDepthFiles(string currentPath)
        {
            string path = ParseRootName(currentPath);
            try
            {
                Task<JsonDataContainer?> task = Task.Run(() => ReadJsonContainerFileContent(path));
                if (task.Wait(MAXWAITTIME))
                {
                    if (task.Result is null) throw new Exception("Invalid return value");
                    return task.Result?.Files!.Select(x => new CloudFile(new FileInfo(Path.Combine(path, x.Key)), x.Value.Owner, x.Value.IsShared, x.Key)).ToList()!;
                }
                else
                {
                    throw new TimeoutException("Unable to manage Container File!");
                }
            }
            catch
            {
                throw new Exception("Error occurred while getting Files!");
            }
        }

        private async Task<JsonDataContainer?> ReadJsonContainerFileContent(string path)
        {
            try
            {
                using FileStream fs = File.Open(Path.Combine(path!, JSONCONTAINERNAME), FileMode.Open, FileAccess.Read, FileShare.Read);
                JsonDataContainer? container = await JsonSerializer.DeserializeAsync<JsonDataContainer>(fs) ?? throw new Exception("Error while importing Folders!");
                return container;
            }
            catch (IOException)
            {
                ; // keep trying until 10 seconds
            }
            catch
            {
                throw;
            }
            return null;
        }

        public List<CloudFolder?> GetCurrentDepthFolders(string currentPath)
        {
            string path = ParseRootName(currentPath);
            try
            {
                Task<JsonDataContainer?> task = Task.Run(() => ReadJsonContainerFileContent(path));
                if (task.Wait(MAXWAITTIME))
                {
                    if (task.Result is null) throw new Exception("Invalid return value");
                    return task.Result?.Folders!.Select(x => new CloudFolder(new DirectoryInfo(Path.Combine(path, x.Key)), x.Value.Owner, x.Value.IsShared)).ToList()!;
                }
                else
                {
                    throw new TimeoutException("Unable to manage Container File!");
                }
            }
            catch
            {
                throw new Exception("Error occurred while getting Folders!");
            }
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
            if (path is null || path == String.Empty)
            {
                throw new ArgumentException("Invalid path!");
            }
            if (folderName is null || folderName == String.Empty)
            {
                throw new ArgumentException("Invalid Folder Name!");
            }
            RemoveFolderFromJsonContainerFile(path, folderName);
            Directory.Delete(pathAndName, true);
            return true;
        }

        private void RemoveFolderFromJsonContainerFile(string path, string folderName)
        {
            try
            {
                Task task = Task.Run(async () =>
                {
                    try
                    {
                        using FileStream fs = File.Open(Path.Combine(path!, JSONCONTAINERNAME), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        JsonDataContainer? container = await JsonSerializer.DeserializeAsync<JsonDataContainer>(fs) ?? throw new Exception("Error while importing Folder Data!");
                        container.Folders?.Remove(folderName!);
                        fs.SetLength(0);
                        await JsonSerializer.SerializeAsync(fs, container);
                    }
                    catch (IOException)
                    {
                        ; // keep trying until 10 seconds
                    }
                    catch
                    {
                        throw;
                    }
                });
                if (!task.Wait(MAXWAITTIME))
                {
                    throw new TimeoutException("Unable to manage Container File!");
                }
            }
            catch (Exception ex) when (ex.InnerException is ArgumentException || ex.InnerException is TimeoutException)
            {
                throw;
            }
            catch
            {
                throw new Exception("Error occurred while removing Folder!");
            }
        }


        public bool RemoveFile(string fileName, string currentPath)
        {
            string path = ParseRootName(currentPath);
            string pathAndName = Path.Combine(path, fileName);
            if (!File.Exists(pathAndName))
            {
                throw new Exception("The File does not exists!");
            }
            if (path is null || path == String.Empty)
            {
                throw new ArgumentException("Invalid path!");
            }
            if (fileName is null || fileName == String.Empty)
            {
                throw new ArgumentException("Invalid Folder Name!");
            }
            try
            {
                RemoveFileFromJsonContainerFile(path, fileName);
                File.Delete(pathAndName);
            }
            catch
            {
                throw new Exception("Could not remove file");
            }
            return true;
        }
        private void RemoveFileFromJsonContainerFile(string path, string fileName)
        {
            try
            {
                Task task = Task.Run(async () =>
                {
                    try
                    {
                        using FileStream fs = File.Open(Path.Combine(path!, JSONCONTAINERNAME), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                        JsonDataContainer? container = await JsonSerializer.DeserializeAsync<JsonDataContainer>(fs) ?? throw new Exception("Error while importing File Data!");
                        container.Files?.Remove(fileName!);
                        fs.SetLength(0);
                        await JsonSerializer.SerializeAsync(fs, container);
                    }
                    catch (IOException)
                    {
                        ; // keep trying until 10 seconds
                    }
                    catch
                    {
                        throw;
                    }
                });
                if (!task.Wait(MAXWAITTIME))
                {
                    throw new TimeoutException("Unable to manage Container File!");
                }
            }
            catch (Exception ex) when (ex.InnerException is ArgumentException || ex.InnerException is TimeoutException)
            {
                throw;
            }
            catch
            {
                throw new Exception("Error occurred while removing Folder!");
            }
        }
        private string ParseRootName(string currentPath)
        {
            if (currentPath.StartsWith(USERROOTNAME))
            {
                return currentPath.Replace(USERROOTNAME, Path.Combine(env.WebRootPath, "CloudData", "UserData"));
            }
            else
            {
                return currentPath.Replace(SHAREDROOTNAME, Path.Combine(env.WebRootPath, "CloudData", "Public"));
            }
        }

        private bool IsSystemFolder(string path)
        {
            List<string> pathFolders = path.Split('\\').ToList();
            return systemFolders.Contains(pathFolders[pathFolders.FindIndex(x => x == "wwwroot") + DISTANCE]);
        }

        public string ReturnServerPath(string currentPath)
        {
            return ParseRootName(currentPath);
        }

        public Tuple<List<CloudFile?>, List<CloudFolder?>> GetCurrentUserIndexData()
        {
            return new Tuple<List<CloudFile?>, List<CloudFolder?>>(new(), new());
        }

        public bool DirectoryExists(string? pathAndName)
        {
            if (pathAndName is null) return false;
            return Directory.Exists(ParseRootName(pathAndName));
        }
    }
}
