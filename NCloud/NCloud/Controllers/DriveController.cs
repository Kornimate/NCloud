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
using static NuGet.Packaging.PackagingConstants;

namespace NCloud.Controllers
{
    public class DriveController : CloudControllerDefault
    {
        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier)
        {
        }

        // GET: DriveController/Details/5
        public IActionResult Details(string? folderName = null)
        {
            PathData pathdata = GetSessionUserPathData();
            string currentPath = String.Empty;
            if (service.DirectoryExists(pathdata.TrySetFolder(folderName)))
            {
                currentPath = pathdata.SetFolder(folderName);
            }
            else
            {
                currentPath = pathdata.CurrentPath;
            }
            SetSessionUserPathData(pathdata);
            try
            {
                return View(new DriveDetailsViewModel(service.GetCurrentDepthFiles(currentPath),
                                                service.GetCurrentDepthFolders(currentPath),
                                                                pathdata.CurrentPathShow));
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));
                return View(new DriveDetailsViewModel(new List<CloudFile?>(),
                                                    new List<CloudFolder?>(),
                                                    pathdata.CurrentPathShow));
            }
        }

        public IActionResult Back()
        {
            PathData pathdata = GetSessionUserPathData();
            if (pathdata.PreviousDirectories.Count > 2)
            {
                pathdata.RemoveFolderFromPrevDirs();
                SetSessionUserPathData(pathdata);
            }
            return RedirectToAction("Details", "Drive");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFolder(string? folderName)
        {
            try
            {
                if (folderName == JSONCONTAINERNAME)
                {
                    throw new Exception("Invalid Folder Name!");
                }

                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one character!");
                }

                service.CreateDirectory(folderName!, GetSessionUserPathData().CurrentPath, (await userManager.GetUserAsync(User)).UserName);

                AddNewNotification(new Success("Folder is created!"));
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));
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
                AddNewNotification(new Warning("No Files were uploaded!"));
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
                int res = await service.CreateFile(files[i], pathData.CurrentPath, (await userManager.GetUserAsync(User)).UserName);
                if (res == 0)
                {
                    AddNewNotification(new Warning($"A File has been renamed!"));
                }
                else if (res == -1)
                {
                    errorPresent = true;
                    AddNewNotification(new Error($"There was error adding the file {files[i].FileName}!"));
                }
            }
            if (!errorPresent)
            {
                AddNewNotification(new Success($"File{(files.Count > 1 ? "s" : "")} added successfully!"));
            }
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult DeleteFolder(string folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one character!");
                }
                if (!service.RemoveDirectory(folderName!, GetSessionUserPathData().CurrentPath))
                {
                    throw new Exception("Folder is System Folder!");
                }
                AddNewNotification(new Success("Folder is removed!"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Failed to remove Folder!"));
            }
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult DeleteFile(string fileName)
        {
            try
            {
                if (fileName is null || fileName == String.Empty)
                {
                    throw new Exception("File name must be at least one character!");
                }
                if (!service.RemoveFile(fileName!, GetSessionUserPathData().CurrentPath))
                {
                    throw new Exception("File is System Folder!");
                }
                AddNewNotification(new Success("File is removed!"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Failed to remove Folder!"));
            }
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult DeleteItems()
        {
            PathData pathData = GetSessionUserPathData();
            try
            {
                var files = service.GetCurrentDepthFiles(pathData.CurrentPath);
                var folders = service.GetCurrentDepthFolders(pathData.CurrentPath);
                return View(new DriveDeleteViewModel
                {
                    Folders = folders,
                    Files = files,
                    ItemsForDelete = new string[files.Count + folders.Count].ToList()
                });
            }
            catch(Exception ex)
            {
                AddNewNotification(new Error(ex.Message));
                return View(new DriveDeleteViewModel
                {
                    Folders = new List<CloudFolder?>(),
                    Files = new List<CloudFile?>(),
                    ItemsForDelete = Array.Empty<string>().ToList()
                });
            }
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
                                AddNewNotification(new Error($"Error removing file {itemName}"));
                                noFail = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            AddNewNotification(new Error($"{ex.Message} ({itemName})"));
                            noFail = false;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (!service.RemoveDirectory(itemName, pathData.CurrentPath))
                            {
                                AddNewNotification(new Error($"Error removing folder {itemName}"));
                                noFail = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            AddNewNotification(new Error($"{ex.Message} ({itemName})"));
                            noFail = false;
                        }
                    }
                }
            }
            if (noFail)
            {
                AddNewNotification(new Success("All Items removed successfully!"));
            }
            else
            {
                AddNewNotification(new Warning("Could not complete all item deletion!"));
            }
            return RedirectToAction("DeleteItems", "Drive");
        }
        public IActionResult DownloadItems()
        {
            PathData pathData = GetSessionUserPathData();
            try
            {
                var files = service.GetCurrentDepthFiles(pathData.CurrentPath);
                //var folders = service.GetCurrentDepthFolders(pathData.CurrentPath); //later to be able to add folders to zp too
                var folders = new List<CloudFolder?>();
                return View(new DriveDownloadViewModel
                {
                    Folders = folders,
                    Files = files,
                    ItemsForDownload = new string[files.Count + folders.Count].ToList()
                });
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));
                return View(new DriveDownloadViewModel
                {
                    Folders = new List<CloudFolder?>(),
                    Files = new List<CloudFile?>(),
                    ItemsForDownload = Array.Empty<string>().ToList()
                });
            }
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
                                    AddNewNotification(new Error($"{ex.Message} ({itemName})"));
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
                                    AddNewNotification(new Error($"{ex.Message} ({itemName})"));
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

        public IActionResult PubliciseFolder(string folderName)
        {
            return RedirectToAction("Details", "Drive");
        }

        public IActionResult PubliciseFile(string fileName)
        {
            return RedirectToAction("Details", "Drive");
        }
    }
}
