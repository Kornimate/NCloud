using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using NToastNotify;
using System.IO.Compression;
using System.Text.Json;

namespace NCloud.Controllers
{
    public class SharingController : CloudControllerDefault
    {
        public SharingController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier, IToastNotification toastNotification) : base(service, userManager, signInManager, env, notifier) { }
        public IActionResult Details(string? folderName = null, List<string>? notifications = null)
        {
            HandleNotifications(notifications);
            SharedData pathdata = GetSessionSharedPathData();
            string currentPath = pathdata.SetFolder(folderName);
            SetSessionSharedPathData(pathdata);
            if (pathdata.CurrentPath != SharedData.ROOTNAME)
            {
                ViewBag.CanUseActions = true;
            }
            return View(new DriveDetailsViewModel(service.GetCurrentDeptFiles(currentPath),
                                                service.GetCurrentDeptFolders(currentPath),
                                                                pathdata.CurrentPathShow));
        }

        public IActionResult Back()
        {
            SharedData pathdata = GetSessionSharedPathData();
            pathdata.RemoveFolderFromPrevDirs();
            SetSessionSharedPathData(pathdata);
            return RedirectToAction("Details", "Sharing");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFolder(string? folderName)
        {
            CloudNotifierService nservice = new CloudNotifierService();
            try
            {
                if (folderName == JSONCONTAINERNAME)
                {
                    throw new Exception("Invalid Folder Name!");
                }
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if (!service.CreateDirectory(folderName!, GetSessionSharedPathData().CurrentPath, (await userManager.GetUserAsync(User)).UserName))
                {
                    throw new Exception("Unknown Error occured");
                }
                nservice.AddNotification("Folder is created!", NotificationType.SUCCESS);
            }
            catch (Exception ex)
            {
                nservice.AddNotification(ex.Message, NotificationType.ERROR);
            }
            return RedirectToAction("Details", new { notifications = nservice.Notifications });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFiles(List<IFormFile>? files = null)
        {
            bool errorPresent = false;
            if (files == null || files.Count == 0)
            {
                notifier.Warning("No Files were uploaded!");
                return RedirectToAction("Details", "Sharing");
            }
            SharedData pathData = GetSessionSharedPathData();
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
                int res = await service.CreateFile(files[i], pathData.CurrentPath, (await userManager.GetUserAsync(User)).UserName);
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
            return RedirectToAction("Details", "Sharing");
        }

        public IActionResult DeleteFolder(string folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if (!service.RemoveDirectory(folderName!, GetSessionSharedPathData().CurrentPath))
                {
                    throw new Exception("Folder is System Folder!");
                }
                notifier.Success("Folder is removed!");
            }
            catch (Exception)
            {
                notifier.Error("Failed to remove Folder!");
            }
            return RedirectToAction("Details", "Sharing");
        }

        public IActionResult DeleteFile(string fileName)
        {
            try
            {
                if (fileName is null || fileName == String.Empty)
                {
                    throw new Exception("File name must be at least one charachter!");
                }
                if (!service.RemoveFile(fileName!, GetSessionSharedPathData().CurrentPath))
                {
                    throw new Exception("File is System Folder!");
                }
                notifier.Success("File is removed!");
            }
            catch (Exception)
            {
                notifier.Error("Failed to remove Folder!");
            }
            return RedirectToAction("Details", "Sharing");
        }

        public IActionResult DeleteItems()
        {
            SharedData pathData = GetSessionSharedPathData();
            if (pathData.CurrentPath == SharedData.ROOTNAME)
            {
                //TODO: notification
                return RedirectToAction("Details", "Sharing");
            }
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
            SharedData pathData = GetSessionSharedPathData();
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
            return RedirectToAction("DeleteItems", "Sharing");
        }
        public IActionResult DownloadItems()
        {
            SharedData pathData = GetSessionSharedPathData();
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
            SharedData pathData = GetSessionSharedPathData();
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
    }
}
