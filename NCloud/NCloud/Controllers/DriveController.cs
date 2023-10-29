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
using Castle.Core;
using NToastNotify;

namespace NCloud.Controllers
{
    public class DriveController : CloudControllerDefault
    {
        private readonly IToastNotification _toastNotification;
        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier, IToastNotification toastNotification) : base(service, userManager, signInManager, env, notifier)
        {
            _toastNotification = toastNotification;
        }

        // GET: DriveController
        public async Task<ActionResult> Index()
        {
            await signInManager.PasswordSignInAsync("Admin", "Admin_1234", true, false);
            var result = service.GetCurrentUserIndexData();
            _toastNotification.AddInfoToastMessage("Hello");
            notifier.Success("Hello");
            return View(new DriveIndexViewModel(result.Item1, result.Item2)
            {
                //TestString = Url.Action("Index", "Drive",new { path="testpath" },Request.Scheme)
            });
        }

        // GET: DriveController/Details/5
        public async Task<ActionResult> Details(string? folderName = null, List<string>? notifications = null)
        {
            HandleNotifications(notifications);
            PathData pathdata = null!;
            if (folderName is null)
            {
                if (HttpContext.Session.Keys.Contains(USERCOOKIENAME))
                {
                    pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(USERCOOKIENAME)!)!;
                }
                else
                {
                    pathdata = new PathData();
                    CloudUser user = await userManager.GetUserAsync(User);
                    pathdata.SetDefaultPathData(user.Id.ToString());
                }
            }
            else
            {
                if (HttpContext.Session.Keys.Contains(USERCOOKIENAME))
                {
                    pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(USERCOOKIENAME)!)!;
                }
                else
                {
                    return BadRequest();
                }
            }
            string currentPath = pathdata.SetFolder(folderName);
            HttpContext.Session.SetString(USERCOOKIENAME, JsonSerializer.Serialize<PathData>(pathdata));
            ViewBag.CurrentPath = pathdata.CurrentPathShow;
            return View(new DriveDetailsViewModel(service.GetCurrentDeptFiles(currentPath),
                                                service.GetCurrentDeptFolders(currentPath),
                                                                              currentPath));
        }

        public IActionResult Back()
        {
            PathData pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(USERCOOKIENAME)!)!;
            if (pathdata.PreviousDirectories.Count > 2)
            {
                pathdata.RemoveFolderFromPrevDirs();
                HttpContext.Session.SetString(USERCOOKIENAME, JsonSerializer.Serialize<PathData>(pathdata));
            }
            return RedirectToAction("Details", "Drive");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFolder(string? folderName)
        {
            CloudNotifierService nservice = new CloudNotifierService();
            try
            {
                if(folderName == JSONCONTAINERNAME)
                {
                    throw new Exception("Invalid Folder Name!");
                }
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if (!service.CreateDirectory(folderName!, GetSessionUserPathData().CurrentPath, (await userManager.GetUserAsync(User)).UserName))
                {
                    throw new Exception("Unknown Error occured");
                }
                nservice.AddNotification("Folder is created!", NotificationType.SUCCESS);
            }
            catch (Exception ex)
            {
                nservice.AddNotification(ex.Message, NotificationType.ERROR);
            }
            return RedirectToAction("Details",new { notifications = nservice.Notifications });
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
            PathData pathData = GetSessionUserPathData();
            for (int i = 0; i < files.Count; i++)
            {
                if (files[i].FileName == JSONCONTAINERNAME)
                {
                    //TODO:notify
                    continue;
                }
                FileInfo fi = new FileInfo(files[i].FileName);
                // TODO: check if allowed filetype

            }
            for (int i = 0; i < files.Count; i++)
            {
                int res = await service.CreateFile(files[i], pathData.CurrentPath,(await userManager.GetUserAsync(User)).UserName);
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
                if (!service.RemoveDirectory(folderName!, GetSessionUserPathData().CurrentPath))
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
                if (!service.RemoveFile(fileName!, GetSessionUserPathData().CurrentPath))
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
            PathData pathData = GetSessionUserPathData();
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
            PathData pathData = GetSessionUserPathData();
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
            PathData pathData = GetSessionUserPathData();
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
            PathData pathData = GetSessionUserPathData();
            string tempFile = Path.GetTempFileName();
            using var zipFile = System.IO.File.Create(tempFile);
            if (vm.ItemsForDownload is not null && vm.ItemsForDownload.Count != 0)
            {
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    foreach (string itemName in vm.ItemsForDownload!)
                    {
                        if (itemName != "false")
                        {
                            if (itemName[0] == '_')
                            {
                                try
                                {
                                    string name = itemName[1..];
                                    archive.CreateEntryFromFile(Path.Combine(service.ReturnServerPath(pathData.CurrentPath), name), name);
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
            //warning implementation
            return RedirectToAction("DownloadItems");
        }

        public IActionResult Terminal()
        {
            PathData pathData = GetSessionUserPathData();
            return View(new TerminalViewModel
            {
                CurrentDirectory = pathData.CurrentPathShow
            });
        }
    }
}
