using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.Drawing.Drawing2D;
using System.Text.Json;
using System.IO;
using PathData = NCloud.Models.PathData;
using System.IO.Compression;

namespace NCloud.Controllers
{
    public class DriveController : Controller
    {
        private readonly ICloudService service;
        private readonly IWebHostEnvironment env;
        private readonly INotyfService notifier;
        private readonly UserManager<CloudUser> userManager;
        private readonly SignInManager<CloudUser> signInManager;
        private const string FOLDERSEPARATOR = "//";
        private const string COOKIENAME = "pathData";
        private const string ROOTNAME = "@CLOUDROOT";
        private const string APPNAME = "NCloudDrive";
        private readonly List<string> ALLOWEDFILETYPES = new List<string>();

        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier)
        {
            this.service = service;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
            this.notifier = notifier;
        }

        // GET: DriveController
        public ActionResult Index()
        {
            Task.Run(async () => await signInManager.PasswordSignInAsync("Admin", "Admin_1234", false, false)).Wait();
            var result = service.GetCurrentUserIndexData();
            return View(new DriveIndexViewModel(result.Item1, result.Item2));
        }

        // GET: DriveController/Details/5
        public async Task<ActionResult> Details(string? folderName = null)
        {
            PathData pathdata = null!;
            if (folderName is null)
            {
                if (HttpContext.Session.Keys.Contains(COOKIENAME))
                {
                    pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
                }
                else
                {
                    pathdata = new PathData();
                    pathdata.SetDefaultPathData((await userManager.GetUserAsync(User)).Id.ToString());
                }
            }
            else
            {
                if (HttpContext.Session.Keys.Contains(COOKIENAME))
                {
                    pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
                }
                else
                {
                    return BadRequest();
                }
            }
            string currentPath = pathdata.SetFolder(folderName);
            HttpContext.Session.SetString(COOKIENAME, JsonSerializer.Serialize<PathData>(pathdata));
            ViewBag.CurrentPath = pathdata.CurrentPathShow;
            return View(new DriveDetailsViewModel(service.GetCurrentDeptFiles(currentPath),
                                                service.GetCurrentDeptFolders(currentPath),
                                                                              currentPath));
        }

        public IActionResult Back()
        {
            PathData pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
            if (pathdata.PreviousDirectories.Count > 2)
            {
                pathdata.RemoveFolderFromPrevDirs();
                HttpContext.Session.SetString(COOKIENAME, JsonSerializer.Serialize<PathData>(pathdata));
            }
            return RedirectToAction("Details", "Drive");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewFolder(string? folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if (!service.CreateDirectory(folderName!, GetSessionPathData().CurrentPath))
                {
                    throw new Exception("Unknown Error occured");
                }
                notifier.Success("Folder is created!");
            }
            catch (Exception ex)
            {
                TempData["FolderError"] = ex.Message;
                notifier.Error("Failed to create Folder!");
            }
            return RedirectToAction("Details");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFiles(List<IFormFile>? files = null)
        {
            bool errorPresent = false;
            if (files == null || files.Count == 0)
            {
                notifier.Warning("No Files were uploaded!");
                return RedirectToAction("Details", "Drive");
            }
            PathData pathData = GetSessionPathData();
            for (int i = 0; i < files.Count; i++)
            {
                FileInfo fi = new FileInfo(files[i].FileName);
                // TODO: check if allowed filetype

            }
            for (int i = 0; i < files.Count; i++)
            {
                int res = await service.CreateFile(files[i], pathData.CurrentPath);
                if (res == 0)
                {
                    notifier.Warning($"A File has been renamed!");
                }
                else if (res == -1)
                {
                    errorPresent = true;
                    notifier.Error($"There was error adding the file {files[i].FileName}!");
                }
            }
            if (!errorPresent)
            {
                notifier.Success($"File{(files.Count > 1 ? "s" : "")} added successfully!");
            }
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult DeleteFolder(string folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if (!service.RemoveDirectory(folderName!, GetSessionPathData().CurrentPath))
                {
                    throw new Exception("Folder is System Folder!");
                }
                notifier.Success("Folder is removed!");
            }
            catch (Exception)
            {
                notifier.Error("Failed to remove Folder!");
            }
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult DeleteFile(string fileName)
        {
            try
            {
                if (fileName is null || fileName == String.Empty)
                {
                    throw new Exception("File name must be at least one charachter!");
                }
                if (!service.RemoveFile(fileName!, GetSessionPathData().CurrentPath))
                {
                    throw new Exception("File is System Folder!");
                }
                notifier.Success("File is removed!");
            }
            catch (Exception)
            {
                notifier.Error("Failed to remove Folder!");
            }
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult DeleteItems()
        {
            PathData pathData = GetSessionPathData();
            var files = service.GetCurrentDeptFiles(pathData.CurrentPath);
            var folders = service.GetCurrentDeptFolders(pathData.CurrentPath);
            return View(new DriveDeleteViewModel
            {
                Folders = folders,
                Files = files,
                ItemsForDelete = new string[files.Count + folders.Count].ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DeleteItems")]
        public IActionResult DeleteItemsFromForm([Bind("ItemsForDelete")] DriveDeleteViewModel vm)
        {
            bool noFail = true;
            PathData pathData = GetSessionPathData();
            foreach (string itemName in vm.ItemsForDelete!)
            {
                if (itemName != "false")
                {
                    if (itemName[0] == '_')
                    {
                        try
                        {
                            if (!service.RemoveFile(itemName[1..], pathData.CurrentPath))
                            {
                                notifier.Error($"Error removing file {itemName}");
                                noFail = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            notifier.Error($"{ex.Message} ({itemName})");
                            noFail = false;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!service.RemoveDirectory(itemName, pathData.CurrentPath))
                            {
                                notifier.Error($"Error removing folder {itemName}");
                                noFail = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            notifier.Error($"{ex.Message} ({itemName})");
                            noFail = false;
                        }
                    }
                }
            }
            if (noFail)
            {
                notifier.Success("All Items removed successfully!");
            }
            else
            {
                notifier.Warning("Could not complete all item deletion!");
            }
            return RedirectToAction("DeleteItems", "Drive");
        }
        public IActionResult DownloadItems()
        {
            PathData pathData = GetSessionPathData();
            var files = service.GetCurrentDeptFiles(pathData.CurrentPath);
            //var folders = service.GetCurrentDeptFolders(pathData.CurrentPath); //later to be able to add folders to zp too
            var folders = new List<CloudFolder?>();
            return View(new DriveDownloadViewModel
            {
                Folders = folders,
                Files = files,
                ItemsForDownload = new string[files.Count + folders.Count].ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DownloadItems")]
        public IActionResult DownloadItemsFromForm([Bind("ItemsForDownload")] DriveDownloadViewModel vm)
        {
            PathData pathData = GetSessionPathData();
            string tempFile = Path.GetTempFileName();
            using var zipFile = System.IO.File.Create(tempFile);
            using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
            {
                foreach (string itemName in vm.ItemsForDownload!)
                {
                    if (itemName != "false")
                    {
                        if (itemName[0] == '_')
                        {
                            string name = itemName[1..];
                            archive.CreateEntryFromFile(Path.Combine(service.ReturnServerPath(pathData.CurrentPath),name), name);
                            try
                            {
                                ;
                            }
                            catch (Exception ex)
                            {
                                notifier.Error($"{ex.Message} ({itemName})");
                            }
                        }
                        else
                        {
                            try
                            {
                                // TODO: here comes folder zipping
                            }
                            catch (Exception ex)
                            {
                                notifier.Error($"{ex.Message} ({itemName})");
                            }
                        }
                    }
                }
            }
            FileStream stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/zip", $"{APPNAME}_{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}.zip");
        }

        public IActionResult Terminal()
        {
            return View();
        }

        [NonAction]
        private PathData GetSessionPathData()
        {
            PathData data = null!;
            if (HttpContext.Session.Keys.Contains(COOKIENAME))
            {
                data = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
            }
            else
            {
                data = new PathData();
                HttpContext.Session.SetString(COOKIENAME, JsonSerializer.Serialize<PathData>(data));
            }
            return data;
        }
    }
}
