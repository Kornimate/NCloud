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
using CloudPathData = NCloud.Models.CloudPathData;
using System.IO.Compression;
using Castle.Core;
using static NuGet.Packaging.PackagingConstants;
using NCloud.ConstantData;
using NCloud.DTOs;

namespace NCloud.Controllers
{
    public class DriveController : CloudControllerDefault
    {
        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        // GET: DriveController/Details/Documents
        public async Task<IActionResult> Details(string? folderName = null)
        {
            CloudPathData pathdata = await GetSessionCloudPathData();

            string currentPath = String.Empty;

            if (service.DirectoryExists(pathdata.TrySetFolder(folderName)))
            {
                currentPath = pathdata.SetFolder(folderName);
            }
            else
            {
                currentPath = pathdata.CurrentPath;
            }

            await SetSessionCloudPathData(pathdata);

            try
            {
                return View(new DriveDetailsViewModel(await service.GetCurrentDepthCloudFiles(currentPath, User),
                                                      await service.GetCurrentDepthCloudDirectories(currentPath, User),
                                                      pathdata.CurrentPathShow,
                                                      pathdata.CurrentPath,
                                                      Constants.GetWebControllerAndActionForDetails(),
                                                      Constants.GetWebControllerAndActionForDownload()));
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));
                return View(new DriveDetailsViewModel(new List<CloudFile>(),
                                                      new List<CloudFolder>(),
                                                      pathdata.CurrentPathShow,
                                                      String.Empty,
                                                      null!,
                                                      null!));
            }
        }

        public async Task<IActionResult> Back()
        {
            CloudPathData pathdata = await GetSessionCloudPathData();

            if (pathdata.PreviousDirectories.Count > 2)
            {
                pathdata.RemoveFolderFromPrevDirs();

                await SetSessionCloudPathData(pathdata);
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

                await service.CreateDirectory(folderName!, (await GetSessionCloudPathData()).CurrentPath, User);

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

            CloudPathData pathData = await GetSessionCloudPathData();

            for (int i = 0; i < files.Count; i++)
            {
                FileInfo fi = new FileInfo(files[i].FileName);
                //TODO: check if allowed filetype

            }

            for (int i = 0; i < files.Count; i++)
            {
                int res = await service.CreateFile(files[i], pathData.CurrentPath, User);

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

        public async Task<IActionResult> DeleteFolder(string folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one character!");
                }

                if (!(await service.RemoveDirectory(folderName!, (await GetSessionCloudPathData()).CurrentPath, User)))
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

        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                if (fileName is null || fileName == String.Empty)
                {
                    throw new Exception("File name must be at least one character!");
                }

                if (!(await service.RemoveFile(fileName!, (await GetSessionCloudPathData()).CurrentPath, User)))
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

        public async Task<IActionResult> DeleteItems()
        {
            CloudPathData pathData = await GetSessionCloudPathData();
            try
            {
                var files = await service.GetCurrentDepthCloudFiles(pathData.CurrentPath, User);
                var folders = await service.GetCurrentDepthCloudDirectories(pathData.CurrentPath, User);

                return View(new DriveDeleteViewModel
                {
                    Folders = folders,
                    Files = files,
                    ItemsForDelete = new string[files.Count + folders.Count].ToList()
                });
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));

                return View(new DriveDeleteViewModel
                {
                    Folders = new List<CloudFolder>(),
                    Files = new List<CloudFile>(),
                    ItemsForDelete = Array.Empty<string>().ToList()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DeleteItems")]
        public async Task<IActionResult> DeleteItemsFromForm([Bind("ItemsForDelete")] DriveDeleteViewModel vm)
        {
            bool noFail = true;

            int falseCounter = 0;

            CloudPathData pathData = await GetSessionCloudPathData();

            foreach (string itemName in vm.ItemsForDelete ?? new())
            {
                if (itemName != Constants.NotSelectedResult)
                {
                    if (itemName[0] == Constants.SelectedFileStarterSymbol)
                    {
                        string itemForDelete = itemName[1..];

                        try
                        {
                            if (!(await service.RemoveFile(itemForDelete, pathData.CurrentPath, User)))
                            {
                                AddNewNotification(new Error($"Error removing file {itemForDelete}"));

                                noFail = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            AddNewNotification(new Error($"{ex.Message} ({itemForDelete})"));

                            noFail = false;
                        }
                    }
                    else if (itemName[0] == Constants.SelectedFolderStarterSymbol)
                    {
                        string itemForDelete = itemName[1..];

                        try
                        {
                            if (!(await service.RemoveDirectory(itemForDelete, pathData.CurrentPath, User)))
                            {
                                AddNewNotification(new Error($"Error removing folder {itemForDelete}"));

                                noFail = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            AddNewNotification(new Error($"{ex.Message} ({itemForDelete})"));

                            noFail = false;
                        }
                    }
                }
                else
                {
                    falseCounter++;
                }
            }

            if (falseCounter == vm.ItemsForDelete?.Count)
            {
                AddNewNotification(new Warning($"No files were chosen for deletion"));

                return RedirectToAction("DeleteItems", "Drive");
            }

            if (noFail)
            {
                AddNewNotification(new Success("All Items removed successfully!"));
            }
            else
            {
                AddNewNotification(new Warning("Could not complete all item deletion!"));
            }

            return RedirectToAction("DeleteItems");
        }

        public async Task<IActionResult> DownloadFolder(string? folderName)
        {
            if (folderName is null || folderName == String.Empty)
            {
                return null!;
            }

            return await DownloadItemsFromForm(new DriveDownloadViewModel
            {
                ItemsForDownload = new List<string>()
                {
                    Constants.SelectedFolderStarterSymbol + folderName
                }
            });
        }

        public async Task<IActionResult> DownloadItems()
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            try
            {
                var files = await service.GetCurrentDepthCloudFiles(pathData.CurrentPath, User);
                var folders = await service.GetCurrentDepthCloudDirectories(pathData.CurrentPath, User);

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
                    Folders = new List<CloudFolder>(),
                    Files = new List<CloudFile>(),
                    ItemsForDownload = Array.Empty<string>().ToList()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm([Bind("ItemsForDownload")] DriveDownloadViewModel vm)
        {

            CloudPathData pathData = await GetSessionCloudPathData();

            string tempFile = Path.GetTempFileName(); //may need to chnage to specific

            //TODO: add to clean up process the path

            using var zipFile = System.IO.File.Create(tempFile);

            if (vm.ItemsForDownload is not null && vm.ItemsForDownload.Count != 0)
            {
                using (ZipArchive archive = new ZipArchive(zipFile, ZipArchiveMode.Create))
                {
                    foreach (string itemName in vm.ItemsForDownload!)
                    {
                        if (itemName != Constants.NotSelectedResult)
                        {
                            if (itemName[0] == Constants.SelectedFileStarterSymbol)
                            {
                                try
                                {
                                    string name = itemName[1..];

                                    archive.CreateEntryFromFile(Path.Combine(service.ServerPath(pathData.CurrentPath), name), name);
                                }
                                catch (Exception ex)
                                {
                                    AddNewNotification(new Error($"{ex.Message} ({itemName})"));
                                }
                            }
                            else if (itemName[0] == Constants.SelectedFolderStarterSymbol)
                            {
                                try
                                {
                                    string name = itemName[1..];
                                    string serverPathStart = service.ServerPath(pathData.CurrentPath);
                                    string currentRelativePath = String.Empty;

                                    Queue<Pair<string, DirectoryInfo>> directories = new(new List<Pair<string, DirectoryInfo>>() { new Pair<string, DirectoryInfo>(currentRelativePath, await service.GetFolderByPath(serverPathStart, name)) });

                                    while (directories.Any())
                                    {
                                        var directoryInfo = directories.Dequeue();

                                        int counter = 0;

                                        currentRelativePath = Path.Combine(directoryInfo.First, directoryInfo.Second.Name);

                                        foreach (CloudFile file in await service.GetCurrentDepthCloudFiles(Path.Combine(serverPathStart, currentRelativePath), User))
                                        {
                                            archive.CreateEntryFromFile(Path.Combine(serverPathStart, currentRelativePath, file.Info.Name), Path.Combine(currentRelativePath, file.Info.Name));

                                            ++counter;
                                        }

                                        foreach (CloudFolder folder in await service.GetCurrentDepthCloudDirectories(Path.Combine(serverPathStart, currentRelativePath), User))
                                        {
                                            directories.Enqueue(new Pair<string, DirectoryInfo>(currentRelativePath, folder.Info));

                                            ++counter;
                                        }

                                        if (counter == 0)
                                        {
                                            archive.CreateEntry(currentRelativePath).ExternalAttributes = Constants.EmptyFolderAttributeNumberZip;
                                        }
                                    }
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

                return File(stream, "application/zip", $"{APPNAME}_{DateTime.Now:yyyy'-'MM'-'dd'T'HH'-'mm'-'ss}.{Constants.CompressedArchiveFileType}");
            }

            AddNewNotification(new Warning($"No files were chosen for download"));

            return RedirectToAction("DownloadItems");
        }

        public async Task<JsonResult> ConnectDirectoryToApp([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.ConnectDirectoryToApp(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside connected to application" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting directory to application!" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConnectDirectoryToWeb([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.ConnectDirectoryToWeb(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside connected to web" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting directory to web!" });
            }
        }

        public async Task<JsonResult> ConnectFileToApp([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.ConnectFileToApp(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "File connected to application" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting file to application!" });
            }
        }

        public async Task<JsonResult> ConnectFileToWeb([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.ConnectFileToWeb(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "File connected to web" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting file to web!" });
            }
        }

        public async Task<IActionResult> DisconnectDirectoryFromApp([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisonnectDirectoryFromApp(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside disconnected from application" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting directory from application" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectDirectoryFromWeb([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisconnectDirectoryFromWeb(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside disconnected from web" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting directory from web!" });
            }
        }

        public async Task<JsonResult> DisconnectFileFromApp([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisonnectFileFromApp(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "File disconnected from application" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from application!" });
            }
        }

        public async Task<JsonResult> DisconnectFileFromWeb([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisonnectFileFromWeb(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "File disconnected from web" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from web!" });
            }
        }
    }
}
