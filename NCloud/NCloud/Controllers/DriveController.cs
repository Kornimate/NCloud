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
using System.IO;
using System.Drawing.Drawing2D;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle Private file and folder requests
    /// </summary>
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
            CloudPathData pathData = await GetSessionCloudPathData();

            string currentPath;

            if (SecurityManager.CheckIfDirectoryExists(service.ServerPath(pathData.TrySetFolder(folderName))))
            {
                currentPath = pathData.SetFolder(folderName);
            }
            else
            {
                currentPath = pathData.CurrentPath;

                if (!SecurityManager.CheckIfDirectoryExists(service.ServerPath(currentPath)))
                {
                    pathData.SetDefaultPathData((await userManager.GetUserAsync(User)).Id.ToString());

                    currentPath = pathData.CurrentPath;

                    AddNewNotification(new Information("The previous cloud path does not exist, user navigated back to home"));
                }
            }

            await SetSessionCloudPathData(pathData);

            try
            {
                return View(new DriveDetailsViewModel(passedItems && files is not null ? files : await service.GetCurrentDepthCloudFiles(currentPath),
                                                      passedItems && folders is not null ? folders : await service.GetCurrentDepthCloudDirectories(currentPath),
                                                      pathData.CurrentPathShow,
                                                      pathData.CurrentPath,
                                                      Constants.GetWebControllerAndActionForDetails(),
                                                      Constants.GetWebControllerAndActionForDownload()));
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));

                return View(new DriveDetailsViewModel(new List<CloudFile>(),
                                                      new List<CloudFolder>(),
                                                      pathData.CurrentPathShow,
                                                      pathData.CurrentPath,
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

            try
            {
                if (pathdata.CanGoBack)
                {
                    pathdata.RemoveFolderFromPrevDirs();

                    await SetSessionCloudPathData(pathdata);
                }
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while moving in file system"));
                throw;
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
                string newFolder = await service.CreateDirectory(folderName!, (await GetSessionCloudPathData()).CurrentPath, await userManager.GetUserAsync(User));

                if (newFolder != folderName)
                    throw new CloudFunctionStopException("error while naming directory");

                AddNewNotification(new Success("Directory is created"));
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error: {ex.Message}"));
            }
            catch (CloudLoggerException ex)
            {
                logger.LogError(ex.Message);

                AddNewNotification(new Error("Error while adding directory"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while adding directory"));
            }

            return RedirectToAction("Details");
        }


        /// <summary>
        /// Action method to add new files to the cloud storage
        /// </summary>
        /// <param name="files">List of IFormFiles from form</param>
        /// <returns>Redirection to details action</returns>
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
                try
                {
                    string newFileName = await service.CreateFile(files[i], pathData.CurrentPath, await userManager.GetUserAsync(User));

                    if (newFileName != files[i].FileName)
                    {
                        AddNewNotification(new Warning($"A file has been renamed! New name: {newFileName}"));
                    }

                }
                catch (CloudFunctionStopException ex)
                {
                    errorPresent = true;

                    AddNewNotification(new Error($"Error - {ex.Message}"));
                }
                catch (CloudLoggerException ex)
                {
                    logger.LogError(ex.Message);

                    errorPresent = true;

                    AddNewNotification(new Error($"There was error adding the file {files[i].FileName}!"));
                }
                catch (Exception)
                {
                    errorPresent = true;

                    AddNewNotification(new Error($"There was error adding the file {files[i].FileName}!"));
                }

            }

            if (!errorPresent)
            {
                AddNewNotification(new Success($"File{(files.Count > 1 ? "s" : "")} added successfully"));
            }

            return RedirectToAction("Details", "Drive");
        }

        /// <summary>
        /// Action method to delete a single folder anc it's content
        /// </summary>
        /// <param name="folderName">The name of the folder</param>
        /// <returns>Redirection to details</returns>
        public async Task<IActionResult> DeleteFolder(string folderName)
        {
            try
            {
                if (!(await service.RemoveDirectory(folderName, (await GetSessionCloudPathData()).CurrentPath, await userManager.GetUserAsync(User))))
                {
                    throw new CloudFunctionStopException("folder is system folder");
                }

                AddNewNotification(new Success("Directory is removed"));
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error($"Error while removing directory"));
            }

            return RedirectToAction("Details", "Drive");
        }

        /// <summary>
        /// Action method to delete a single file
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <returns>Redirection to details</returns>
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                if (!(await service.RemoveFile(fileName!, (await GetSessionCloudPathData()).CurrentPath, await userManager.GetUserAsync(User))))
                {
                    throw new CloudFunctionStopException("error while removing file");
                }

                AddNewNotification(new Success("File is removed"));
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error($"Error while removing file"));
            }

            return RedirectToAction("Details", "Drive");
        }

        /// <summary>
        /// Action method to list items for delete in current state
        /// </summary>
        /// <returns>The view with items in it</returns>
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
                AddNewNotification(new Error($"Error - {ex.Message}"));

                return View(new DriveDeleteViewModel
                {
                    Folders = new List<CloudFolder>(),
                    Files = new List<CloudFile>(),
                    ItemsForDelete = Array.Empty<string>().ToList()
                });
            }
        }

        /// <summary>
        /// Action method to handle post request for item deletion
        /// </summary>
        /// <param name="vm">The form data in a view model</param>
        /// <returns>Redirection to delete items</returns>
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
                            if (!(await service.RemoveFile(itemForDelete, pathData.CurrentPath, await userManager.GetUserAsync(User))))
                            {
                                AddNewNotification(new Error($"Error removing file {itemForDelete}"));

                                noFail = false;
                            }
                        }
                        catch (CloudFunctionStopException ex)
                        {
                            AddNewNotification(new Error($"Error - {ex.Message}"));

                            noFail = false;
                        }
                        catch (Exception)
                        {
                            AddNewNotification(new Error($"Error while removing file: {itemForDelete}"));

                            noFail = false;
                        }
                    }
                    else if (itemName[0] == Constants.SelectedFolderStarterSymbol)
                    {
                        string itemForDelete = itemName[1..];

                        try
                        {
                            if (!(await service.RemoveDirectory(itemForDelete, pathData.CurrentPath, await userManager.GetUserAsync(User))))
                            {
                                AddNewNotification(new Error($"Error removing directory: {itemForDelete}"));

                                noFail = false;
                            }
                        }
                        catch (CloudFunctionStopException ex)
                        {
                            AddNewNotification(new Error($"Error - {ex.Message}"));
                        }
                        catch (Exception)
                        {
                            AddNewNotification(new Error($"Error while removing directory: {itemForDelete}"));
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
                AddNewNotification(new Success("All Items removed successfully"));
            }
            else
            {
                AddNewNotification(new Warning("Could not complete all item deletion"));
            }

            return RedirectToAction("DeleteItems");
        }

        /// <summary>
        /// Action method to handle download folder in current state
        /// </summary>
        /// <param name="folderName">Name of the folder to be downloaded</param>
        /// <returns>Redirection to details and file result</returns>
        public async Task<IActionResult> DownloadFolder(string? folderName)
        {
            if (String.IsNullOrWhiteSpace(folderName))
            {
                AddNewNotification(new Error("No directory name specified"));

                return RedirectToAction("Details", "Drive");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFolderStarterSymbol + folderName
            },
            (await GetSessionCloudPathData()).CurrentPath,
            RedirectToAction("Details", "Drive"));
        }

        /// <summary>
        /// Action method to download file from current state
        /// </summary>
        /// <param name="fileName">Name of the file to be downloaded</param>
        /// <returns>Redirection to details and file result</returns>
        public async Task<IActionResult> DownloadFile(string? fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
            {
                AddNewNotification(new Error("No directory name specified"));

                return RedirectToAction("Details", "Drive");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFileStarterSymbol + fileName
            },
            (await GetSessionCloudPathData()).CurrentPath,
            RedirectToAction("Details", "Drive"));
        }

        /// <summary>
        /// Action method to show downloadable items in current state
        /// </summary>
        /// <returns>View with the downloadable items</returns>
        public async Task<IActionResult> DownloadItems()
        {
            try
            {
                CloudPathData pathData = await GetSessionCloudPathData();

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
                AddNewNotification(new Error($"Error - {ex.Message}"));

                return View(new DriveDownloadViewModel
                {
                    Folders = new List<CloudFolder>(),
                    Files = new List<CloudFile>(),
                    ItemsForDownload = Array.Empty<string>().ToList()
                });
            }
        }

        /// <summary>
        /// Action method to handle prost request to download items
        /// </summary>
        /// <param name="vm">Form data in a view model</param>
        /// <returns>Redirection to downloaditems</returns>
        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm([Bind("ItemsForDownload")] DriveDownloadViewModel vm)
        {
            return await Download(vm.ItemsForDownload ?? new(), (await GetSessionCloudPathData()).CurrentPath, RedirectToAction("Details", "Drive"));
        }

        /// <summary>
        /// Action method to handle folder connection request to app via js
        /// </summary>
        /// <param name="itemName">Name of the folder</param>
        /// <returns>Json with data indication success or fail with message</returns>
        public async Task<JsonResult> ConnectDirectoryToApp([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.ConnectDirectoryToApp(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside connected to application" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while connecting directory to application" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting directory to application" });
            }
        }

        /// <summary>
        /// Action method to handle folder connection request to web via js
        /// </summary>
        /// <param name="itemName">Name of folder</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConnectDirectoryToWeb([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.ConnectDirectoryToWeb(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    SharedFolder sf = await service.GetSharedFolderByPathAndName(session.CurrentPath, itemName);

                    var urlData = Constants.GetWebControllerAndActionForDetails();

                    return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside connected to web", Result = Url.Action(urlData.Second, urlData.First, new { id = sf.Id.ToString() }, HttpContext.Request.Scheme) ?? String.Empty });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while connecting directory to web" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting directory to web" });
            }
        }

        /// <summary>
        /// Action method to handle file connection request to app via js
        /// </summary>
        /// <param name="itemName">Name of file</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConnectFileToApp([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.ConnectFileToApp(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "File connected to application" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while connecting file to application" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting file to application" });
            }
        }

        /// <summary>
        /// Action method to handle file connection request to web via js
        /// </summary>
        /// <param name="itemName">Name of file</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ConnectFileToWeb([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.ConnectFileToWeb(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    SharedFile sf = await service.GetSharedFileByPathAndName(session.CurrentPath, itemName);

                    var urlData = Constants.GetWebControllerAndActionForDetails();

                    return Json(new ConnectionDTO { Success = true, Message = "File is connected to web", Result = Url.Action(urlData.Second, urlData.First, new { id = sf.Id.ToString() }, HttpContext.Request.Scheme) ?? String.Empty });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while connecting file to web" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while connecting file to web" });
            }
        }

        /// <summary>
        /// Action method to handle folder connection deletion request to app via js
        /// </summary>
        /// <param name="itemName">Name of folder</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectDirectoryFromApp([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.DisconnectDirectoryFromApp(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside disconnected from application" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting directory from application" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting directory from application" });
            }
        }

        /// <summary>
        /// Action method to handle folder connection deletion request to web via js
        /// </summary>
        /// <param name="itemName">Name of folder</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectDirectoryFromWeb([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.DisconnectDirectoryFromWeb(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "Directory and items inside disconnected from web" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting directory from web" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting directory from web" });
            }
        }

        /// <summary>
        /// Action method to handle file connection deletion request to app via js
        /// </summary>
        /// <param name="itemName">Name of file</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectFileFromApp([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.DisconnectFileFromApp(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "File disconnected from application" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from application" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from application" });
            }
        }

        /// <summary>
        /// Action method to handle file connection deletion request to web via js
        /// </summary>
        /// <param name="itemName">Name of file</param>
        /// <returns>Json with data indication success or fail with message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectFileFromWeb([FromBody] string itemName)
        {
            try
            {
                CloudPathData session = await GetSessionCloudPathData();

                if (await service.DisconnectFileFromWeb(session.CurrentPath, itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "File disconnected from web" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from web" });
                }
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from web" });
            }
        }

        /// <summary>
        /// Action method to handle folder connection deletion request to web via js form dashboard page
        /// </summary>
        /// <param name="folder">Name of folder</param>
        /// <returns>Redirection to dashboard index action</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectDirectoryFromWebDashboard(string folder)
        {
            string[] pathElements = folder.Split(Constants.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            if (pathElements is null || pathElements.Length < 2)
            {
                AddNewNotification(new Error("Error while disconnecting directory from web (invalid path)"));

                return RedirectToAction("Index", "DashBoard");
            }

            string itemName = pathElements.Last();

            string path = String.Join(Path.DirectorySeparatorChar, pathElements.SkipLast(1)); //remove directory from path

            try
            {
                if (await service.DisconnectDirectoryFromWeb(path, itemName, await userManager.GetUserAsync(User)))
                {
                    AddNewNotification(new Success("Directory and items inside disconnected from web"));
                }
                else
                {
                    AddNewNotification(new Error("Error while disconnecting directory from web"));
                }
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while disconnecting directory from web"));
            }

            return RedirectToAction("Index", "DashBoard");
        }

        /// <summary>
        /// Action method to handle file connection deletion request to web via js from dashboard page
        /// </summary>
        /// <param name="file">Name of file</param>
        /// <returns>Redirection to dashboard index action</returns>
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

            try
            {
                if (await service.DisconnectFileFromWeb(path, itemName, await userManager.GetUserAsync(User)))
                {
                    AddNewNotification(new Success("File disconnected from web"));
                }
                else
                {
                    AddNewNotification(new Error("Error while disconnecting file from web"));
                }
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while disconnecting file from web"));
            }

            return RedirectToAction("Index", "DashBoard");
        }

        /// <summary>
        /// Action method to get a specified folder settings from current state
        /// </summary>
        /// <param name="folderName">Name of folder</param>
        /// <returns>View with folder settings in it</returns>
        public async Task<IActionResult> FolderSettings(string folderName)
        {
            try
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                CloudFolder folder = await service.GetFolder(pathData.CurrentPath, folderName);

                if (folder.Info.Exists)
                    return View(new FolderSettingsViewModel(folder.Info.Name, folder.Info.Name, pathData.CurrentPathShow, folder.IsConnectedToApp, folder.IsConnectedToWeb, folder.Info));

                else
                    throw new CloudFunctionStopException("Directory does not exist");

            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while getting directory data"));

                return RedirectToAction("Details", "Drive");
            }
        }

        /// <summary>
        /// Action method to handle post request to modify folder data
        /// </summary>
        /// <param name="vm">Folder settings encapsulated in a view model</param>
        /// <returns>Redirection to drive details, in case of error the view itself</returns>
        [HttpPost]
        [ActionName("FolderSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FolderSettingsForm([Bind("OldName,NewName,Path,ConnectedToApp,ConnectedToWeb")] FolderSettingsViewModel vm)
        {
            if (ModelState.IsValid)
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                bool noError = true;

                string actualName = vm.OldName!;

                if (vm.NewName != vm.OldName)
                {
                    try
                    {
                        SharedPathData sharedData = await GetSessionSharedPathData();

                        actualName = await service.RenameFolder(pathData.CurrentPath, vm.OldName!, vm.NewName!, sharedData);

                        await SetSessionSharedPathData(sharedData);
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while renaming directory"));

                        noError = false;
                    }
                }

                if (vm.ConnectedToWeb)
                {
                    try
                    {
                        if (!await service.ConnectDirectoryToWeb(pathData.CurrentPath, actualName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (web connect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while connecting directory to web"));

                        noError = false;
                    }
                }
                else
                {
                    try
                    {
                        if (!await service.DisconnectDirectoryFromWeb(pathData.CurrentPath, actualName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (web disconnect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while disconnecting directory from web"));

                        noError = false;
                    }
                }

                if (vm.ConnectedToApp)
                {
                    try
                    {
                        if (!await service.ConnectDirectoryToApp(pathData.CurrentPath, actualName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (app connect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while connecting directory to app"));

                        noError = false;
                    }
                }
                else
                {
                    try
                    {
                        if (!await service.DisconnectDirectoryFromApp(pathData.CurrentPath, actualName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (app disconnect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while disconnecting directory from app"));

                        noError = false;
                    }
                }

                if (noError)
                {
                    AddNewNotification(new Success("Directory edited successfully"));
                }

                return RedirectToAction("Details", "Drive");
            }

            AddNewNotification(new Error("Error - invalid input data"));

            return View(vm);
        }

        /// <summary>
        /// Action method to get a specified file settings from current state
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>View with file settings in it</returns>
        public async Task<IActionResult> FileSettings(string fileName)
        {
            try
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                CloudFile file = await service.GetFile(pathData.CurrentPath, fileName);

                if (file.Info.Exists)
                    return View(new FileSettingsViewModel(file.Info.Name, Path.GetFileNameWithoutExtension(file.Info.Name) ?? String.Empty, Path.GetExtension(file.Info.Name), pathData.CurrentPathShow, file.IsConnectedToApp, file.IsConnectedToWeb, file.Info));

                else
                    throw new CloudFunctionStopException("File does not exist");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while getting file data"));

                return RedirectToAction("Details", "Drive");
            }
        }

        /// <summary>
        /// Action method to handle post request to modify file data
        /// </summary>
        /// <param name="vm">File settings encapsulated in a view model</param>
        /// <returns>Redirection to drive details, in case of error the view itself</returns>
        [HttpPost]
        [ActionName("FileSettings")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FileSettingsForm([Bind("OldName,NewName,Extension,Path,ConnectedToApp,ConnectedToWeb")] FileSettingsViewModel vm)
        {
            if (ModelState.IsValid)
            {
                CloudPathData pathData = await GetSessionCloudPathData();

                bool noError = true;

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
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error("Error while renaming file"));

                        noError = false;
                    }
                }

                if (vm.ConnectedToWeb)
                {
                    try
                    {
                        if (!await service.ConnectFileToWeb(pathData.CurrentPath, newFileName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (web connect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while connecting file to web"));

                        noError = false;
                    }
                }
                else
                {
                    try
                    {
                        if (!await service.DisconnectFileFromWeb(pathData.CurrentPath, newFileName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (web disconnect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while disconnecting file from web"));

                        noError = false;
                    }
                }

                if (vm.ConnectedToApp)
                {
                    try
                    {
                        if (!await service.ConnectFileToApp(pathData.CurrentPath, newFileName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (app connect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while connecting file to app"));

                        noError = false;
                    }
                }
                else
                {
                    try
                    {
                        if (!await service.DisconnectFileFromApp(pathData.CurrentPath, newFileName, await userManager.GetUserAsync(User)))
                        {
                            AddNewNotification(new Error("Error while applying settings (app disconnect)"));

                            noError = false;
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        noError = false;
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error($"Error while disconnecting file from app"));

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

        /// <summary>
        /// Action method to handle JS request to copy folder to cloud clioboard
        /// </summary>
        /// <param name="itemName">Name of folder</param>
        /// <returns>Json with boolean (success/false) and messge indicating the success of task</returns>
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

        /// <summary>
        /// Action method to handle JS request to copy file to cloud clioboard
        /// </summary>
        /// <param name="itemName">Name of file</param>
        /// <returns>Json with boolean (success/false) and messge indicating the success of task</returns>
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

        /// <summary>
        /// Action method to paste data from cloud clipboard to current state
        /// </summary>
        /// <returns>Redirection to details</returns>
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

                if (item.IsFile()) //strategy design pattern
                {
                    try
                    {
                        string result = await service.CopyFile(item.ItemPath ?? String.Empty, pathData.CurrentPath, await userManager.GetUserAsync(User));

                        if (result != String.Empty)
                            AddNewNotification(new Warning("Copied file has been renamed"));

                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        return RedirectToAction("Details", "Drive");
                    }
                    catch (Exception)
                    {
                        AddNewNotification(new Error("Error while pasting file"));

                        return RedirectToAction("Details", "Drive");
                    }

                }
                else if (item.IsFolder())
                {
                    try
                    {
                        string result = await service.CopyFolder(item.ItemPath ?? string.Empty, pathData.CurrentPath, await userManager.GetUserAsync(User));

                        if (result != String.Empty)
                        {
                            AddNewNotification(new Warning("Directory has been renamed"));
                        }
                    }
                    catch (CloudFunctionStopException ex)
                    {
                        AddNewNotification(new Error($"Error - {ex.Message}"));

                        return RedirectToAction("Details", "Drive");
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
            catch (MissingMemberException ex)
            {
                AddNewNotification(new Error(ex.Message));

                return RedirectToAction("Details", "Drive");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while pasting item"));

                return RedirectToAction("Details", "Drive");
            }
        }

        /// <summary>
        /// Method to generate QRCode for given url
        /// </summary>
        /// <param name="url">Url for QR code generation</param>
        /// <returns>The image tag src string with base64 string formatted image (the QR code)</returns>

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> GetQRCodeForItem([FromBody] string url)
        {
            try
            {
                return await Task.FromResult<JsonResult>(new JsonResult(new ConnectionDTO { Success = true, Message = "The QR code is generated for item", Result = CloudQRManager.GenerateQRCodeString(url) }));
            }
            catch (Exception)
            {
                return await Task.FromResult<JsonResult>(new JsonResult(new ConnectionDTO { Success = false, Message = "Error while generating QR code" }));
            }
        }
    }
}
