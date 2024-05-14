using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using NCloud.ConstantData;
using NCloud.DTOs;
using NCloud.Security;
using Microsoft.AspNetCore.Authorization;
using NCloud.Services.Exceptions;

namespace NCloud.Controllers
{
    [Authorize]
    public class DriveController : CloudControllerDefault
    {
        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        /// <summary>
        ///  Action method lists the current directory's items
        /// </summary>
        /// <param name="folderName">Name of the directory to be listed</param>
        /// <param name="files">files to display on UI if passed as parameter, optional</param>
        /// <param name="folders">folders to display on if passed as parameter, optional</param>
        /// <param name="passedItems">bool if passed items shoold be displayed on UI</param>
        /// <returns>The View with the specified elements or current path elements</returns>
        public async Task<IActionResult> Details(string? folderName = null, List<CloudFile>? files = null, List<CloudFolder>? folders = null, bool passedItems = false)
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
                return View(new DriveDetailsViewModel(passedItems && files is not null ? files : await service.GetCurrentDepthCloudFiles(currentPath),
                                                      passedItems && folders is not null ? folders : await service.GetCurrentDepthCloudDirectories(currentPath),
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
                                                      pathdata.CurrentPath,
                                                      null!,
                                                      null!));
            }
        }

        /// <summary>
        /// Action method to move up a directory from current state
        /// </summary>
        /// <returns>Redirection to Details action with adjusted current state</returns>
        public async Task<IActionResult> Back()
        {
            CloudPathData pathdata = await GetSessionCloudPathData();

            if (pathdata.CanGoBack)
            {
                pathdata.RemoveFolderFromPrevDirs();

                await SetSessionCloudPathData(pathdata);
            }

            return RedirectToAction("Details", "Drive");
        }

        /// <summary>
        /// Action method to navigate back to root
        /// </summary>
        /// <returns>Redirection to Details action with adjusted current state</returns>
        public async Task<IActionResult> Home()
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            pathData.SetDefaultPathData((await userManager.GetUserAsync(User)).Id.ToString());

            await SetSessionCloudPathData(pathData);

            return RedirectToAction("Details", "Drive");
        }

        /// <summary>
        /// Action method to create new physical folder in user's space
        /// </summary>
        /// <param name="folderName">name of the folder to be created</param>
        /// <returns>Redirection to details with current state</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNewFolder(string? folderName)
        {
            try
            {
                await service.CreateDirectory(folderName!, (await GetSessionCloudPathData()).CurrentPath, await userManager.GetUserAsync(User));

                AddNewNotification(new Success("directory is created"));
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error: {ex.Message}"));
            }
            catch (CloudLoggerException ex)
            {
                logger.LogError(ex.Message);

                AddNewNotification(new Error("Error while executing action"));
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
                string res = await service.CreateFile(files[i], pathData.CurrentPath, User);

                if (res != files[i].FileName)
                {
                    AddNewNotification(new Warning($"A file has been renamed!"));
                }
                else if (res == String.Empty)
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
                AddNewNotification(new Error("Failed to remove File!"));
            }

            return RedirectToAction("Details", "Drive");
        }

        public async Task<IActionResult> DeleteItems()
        {
            CloudPathData pathData = await GetSessionCloudPathData();
            try
            {
                var files = await service.GetCurrentDepthCloudFiles(pathData.CurrentPath);
                var folders = await service.GetCurrentDepthCloudDirectories(pathData.CurrentPath);

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
                return View("Error");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFolderStarterSymbol + folderName
            },
            (await GetSessionCloudPathData()).CurrentPath,
            RedirectToAction("Details", "Drive"));
        }

        public async Task<IActionResult> DownloadFile(string? fileName)
        {
            if (fileName is null || fileName == String.Empty)
            {
                return View("Error");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFileStarterSymbol + fileName
            },
            (await GetSessionCloudPathData()).CurrentPath,
            RedirectToAction("Details", "Drive"));
        }

        public async Task<IActionResult> DownloadItems()
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            try
            {
                var files = await service.GetCurrentDepthCloudFiles(pathData.CurrentPath);
                var folders = await service.GetCurrentDepthCloudDirectories(pathData.CurrentPath);

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
            return await Download(vm.ItemsForDownload ?? new(), (await GetSessionCloudPathData()).CurrentPath, RedirectToAction("Details", "Drive"));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectDirectoryFromApp([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisconnectDirectoryFromApp(session.CurrentPath, itemName, User))
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectFileFromApp([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisconnectFileFromApp(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "File disconnected from application" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from application!" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectFileFromWeb([FromBody] string itemName)
        {
            CloudPathData session = await GetSessionCloudPathData();

            if (await service.DisconnectFileFromWeb(session.CurrentPath, itemName, User))
            {
                return Json(new ConnectionDTO { Success = true, Message = "File disconnected from web" });
            }
            else
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from web!" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectDirectoryFromWebDashboard(string folder)
        {
            string[] pathElements = folder.Split(Constants.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (pathElements is null || pathElements.Length < 2)
            {
                AddNewNotification(new Error("Error while disconnecting directory from web! (invalid path)"));

                return RedirectToAction("Index", "DashBoard");
            }

            string itemName = pathElements.Last();

            string path = String.Join(Path.DirectorySeparatorChar, pathElements.SkipLast(1)); //remove directory from path

            if (await service.DisconnectDirectoryFromWeb(path, itemName, User))
            {
                AddNewNotification(new Success("Directory and items inside disconnected from web"));
            }
            else
            {
                AddNewNotification(new Error("Error while disconnecting directory from web!"));
            }

            return RedirectToAction("Index", "DashBoard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectFileFromWebDashboard(string file)
        {
            string[] pathElements = file.Split(Constants.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (pathElements is null || pathElements.Length < 2)
            {
                AddNewNotification(new Error("Error while disconnecting file from web! (invalid path)"));

                return RedirectToAction("Index", "DashBoard");
            }

            string itemName = pathElements.Last();

            string path = String.Join(Path.DirectorySeparatorChar, pathElements.SkipLast(1)); //remove file from path

            if (await service.DisconnectFileFromWeb(path, itemName, User))
            {
                AddNewNotification(new Success("File disconnected from web"));
            }
            else
            {
                AddNewNotification(new Error("Error while disconnecting file from web!"));
            }

            return RedirectToAction("Index", "DashBoard");
        }

        public async Task<IActionResult> FolderSettings(string folderName)
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            if (!SecurityManager.CheckIfDirectoryExists(Path.Combine(service.ServerPath(pathData.CurrentPath), folderName)))
            {
                AddNewNotification(new Error("Directory does not exists"));

                return RedirectToAction("Details", "Drive");
            }

            try
            {
                CloudFolder folder = await service.GetFolder(pathData.CurrentPath, folderName);

                return View(new FolderSettingsViewModel(folder.Info.Name, folder.Info.Name, pathData.CurrentPathShow, folder.IsConnectedToApp, folder.IsConnectedToWeb, folder.Info));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Directory does not exists"));

                return RedirectToAction("Details", "Drive");
            }
        }

        [HttpPost]
        [ActionName("FolderSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FolderSettingsForm([Bind("OldName,NewName,Path,ConnectedToApp,ConnectedToWeb")] FolderSettingsViewModel vm)
        {
            if (ModelState.IsValid)
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                bool noError = true;

                if (!SecurityManager.CheckIfDirectoryExists(Path.Combine(service.ServerPath(pathData.CurrentPath), vm.OldName!)))
                {
                    AddNewNotification(new Error("Directory does not exist"));

                    return RedirectToAction("Details", "Drive");
                }

                if (vm.NewName != vm.OldName)
                {
                    string msg = await service.RenameFolder(pathData.CurrentPath, vm.OldName!, vm.NewName!);

                    if (msg != String.Empty)
                    {
                        AddNewNotification(new Error(msg));

                        return RedirectToAction("Details", "Drive");
                    }
                }

                if (vm.ConnectedToWeb)
                {
                    if (!await service.ConnectDirectoryToWeb(pathData.CurrentPath, vm.NewName!, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (web connect)"));

                        noError = false;
                    }
                }
                else
                {
                    if (!await service.DisconnectDirectoryFromWeb(pathData.CurrentPath, vm.NewName!, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (web disconnect)"));

                        noError = false;
                    }
                }

                if (vm.ConnectedToApp)
                {
                    if (!await service.ConnectDirectoryToApp(pathData.CurrentPath, vm.NewName!, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (app connect)"));

                        noError = false;
                    }
                }
                else
                {
                    if (!await service.DisconnectDirectoryFromApp(pathData.CurrentPath, vm.NewName!, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (app disconnect)"));

                        noError = false;
                    }
                }

                if (noError)
                {
                    AddNewNotification(new Success("Directory edited successfully"));
                }

                return RedirectToAction("Details", "Drive");
            }

            AddNewNotification(new Error("Invalid input data"));

            return View(vm);
        }

        public async Task<IActionResult> FileSettings(string fileName)
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            if (!SecurityManager.CheckIfFileExists(Path.Combine(service.ServerPath(pathData.CurrentPath), fileName)))
            {
                AddNewNotification(new Error("File does not exists"));

                return RedirectToAction("Details", "Drive");
            }

            try
            {
                CloudFile file = await service.GetFile(pathData.CurrentPath, fileName);

                return View(new FileSettingsViewModel(file.Info.Name, Path.GetFileNameWithoutExtension(file.Info.Name) ?? String.Empty, Path.GetExtension(file.Info.Name), pathData.CurrentPathShow, file.IsConnectedToApp, file.IsConnectedToWeb, file.Info));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("File does not exists"));

                return RedirectToAction("Details", "Drive");
            }
        }

        [HttpPost]
        [ActionName("FileSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileSettingsForm([Bind("OldName,NewName,Extension,Path,ConnectedToApp,ConnectedToWeb")] FileSettingsViewModel vm)
        {
            if (ModelState.IsValid)
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                bool noError = true;

                if (!SecurityManager.CheckIfFileExists(Path.Combine(service.ServerPath(pathData.CurrentPath), vm.OldName!)))
                {
                    AddNewNotification(new Error("File does not exist"));

                    return RedirectToAction("Details", "Drive");
                }

                string newFileName = vm.NewName! + vm.Extension!; // already has the "." delimiter

                if (newFileName != vm.OldName)
                {
                    try
                    {
                        string name = await service.RenameFile(pathData.CurrentPath, vm.OldName!, newFileName);

                        if (name != newFileName)
                        {
                            newFileName = name;
                            
                            AddNewNotification(new Warning("File has been renamed"));
                        }

                    }
                    catch (Exception ex)
                    {
                        AddNewNotification(new Error(ex.Message));

                        return RedirectToAction("Details", "Drive");
                    }
                }

                if (vm.ConnectedToWeb)
                {
                    if (!await service.ConnectFileToWeb(pathData.CurrentPath, newFileName, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (web connect)"));

                        noError = false;
                    }
                }
                else
                {
                    if (!await service.DisconnectFileFromWeb(pathData.CurrentPath, newFileName, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (web disconnect)"));

                        noError = false;
                    }
                }

                if (vm.ConnectedToApp)
                {
                    if (!await service.ConnectFileToApp(pathData.CurrentPath, newFileName, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (app connect)"));

                        noError = false;
                    }
                }
                else
                {
                    if (!await service.DisconnectFileFromApp(pathData.CurrentPath, newFileName, User))
                    {
                        AddNewNotification(new Error("Error while applying settings (app disconnect)"));

                        noError = false;
                    }
                }

                if (noError)
                {
                    AddNewNotification(new Success("File edited successfully"));
                }

                return RedirectToAction("Details", "Drive");
            }

            AddNewNotification(new Error("Invalid input data"));

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CopyFolderToCloudClipboard([FromBody] string itemName)
        {
            if (itemName is null || itemName == String.Empty)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Invalid directory name" });
            }

            try
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                pathData.SetClipBoardData(Path.Combine(pathData.CurrentPath, itemName), false);

                await SetSessionCloudPathData(pathData);

                return Json(new ConnectionDTO { Success = true, Message = "Successfully copied directory to cloud clipboard" });

            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while copy directory to cloud clipboard" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CopyFileToCloudClipboard([FromBody] string itemName)
        {
            if (itemName is null || itemName == String.Empty)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Invalid file name" });
            }

            try
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                pathData.SetClipBoardData(Path.Combine(pathData.CurrentPath, itemName), true);

                await SetSessionCloudPathData(pathData);

                return Json(new ConnectionDTO { Success = true, Message = "Successfully copied file to cloud clipboard" });

            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while copy file to cloud clipboard" });
            }
        }

        public async Task<IActionResult> PasteDataFromClipBoard()
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            try
            {
                CloudRegistration? item = pathData.GetClipBoardData();

                if (item is null)
                {
                    AddNewNotification(new Error("Invalid data in clipboard"));

                    return RedirectToAction("Details", "Drive");
                }

                if (item.IsFile())
                {
                    if (!SecurityManager.CheckIfFileExists(service.ServerPath(item.ItemPath!)))
                    {
                        AddNewNotification(new Error("Source file does not exist"));

                        return RedirectToAction("Details", "Drive");
                    }

                    try
                    {
                        string result = await service.CopyFile(item.ItemPath, pathData.CurrentPath, User);
                        if (result != String.Empty)
                        {
                            AddNewNotification(new Warning("File has been renamed"));
                        }
                    }
                    catch (Exception ex)
                    {
                        AddNewNotification(new Error(ex.Message));

                        return RedirectToAction("Details", "Drive");
                    }

                }
                else if (item.IsFolder())
                {
                    if (!SecurityManager.CheckIfDirectoryExists(service.ServerPath(item.ItemPath!)))
                    {
                        AddNewNotification(new Error("Source directory does not exist"));

                        return RedirectToAction("Details", "Drive");
                    }

                    try
                    {
                        string result = await service.CopyFolder(item.ItemPath, pathData.CurrentPath, User);

                        if (result != String.Empty)
                        {
                            AddNewNotification(new Warning("Directory has been renamed"));
                        }
                    }
                    catch (Exception ex)
                    {
                        AddNewNotification(new Error(ex.Message));

                        return RedirectToAction("Details", "Drive");
                    }

                }
                else
                {
                    AddNewNotification(new Error("Unknown item in cloud clipboard"));

                    return RedirectToAction("Details", "Drive");
                }

                AddNewNotification(new Success("Successfully pasted item"));

                return RedirectToAction("Details", "Drive");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while pasting item"));

                return RedirectToAction("Details", "Drive");
            }
        }
    }
}
