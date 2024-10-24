﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.DTOs;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle sharing page actions
    /// </summary>
    [Authorize]
    public class SharingController : CloudControllerDefault
    {
        public SharingController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        /// <summary>
        /// Action method to show current sharing state items
        /// </summary>
        /// <param name="folderName">Name of folder in current state</param>
        /// <returns>View with current state items (shared files, shared folders)</returns>
        public async Task<IActionResult> Details(string? folderName = null)
        {
            SharedPathData pathdata = await GetSessionSharedPathData();

            string sharedPath = pathdata.CurrentPath;

            try
            {

                if (sharedPath != Constants.PublicRootName)
                {
                    if (await service.SharedPathExists(pathdata.TrySetpath(folderName)))
                    {
                        sharedPath = pathdata.SetFolder(folderName);
                    }
                    else
                    {
                        pathdata = new SharedPathData();

                        sharedPath = pathdata.CurrentPath;

                        AddNewNotification(new Information("The previous shared path does not exist, user navigated back to sharing home"));
                    }

                }
                else if (sharedPath == Constants.PublicRootName)
                {
                    sharedPath = pathdata.SetFolder(folderName);

                }

                await SetSessionSharedPathData(pathdata);

                return View(new SharingDetailsViewModel(await service.GetCurrentDepthAppSharingFiles(sharedPath),
                                                        await service.GetCurrentDepthAppSharingDirectories(sharedPath),
                                                        pathdata.CurrentPathShow,
                                                        await service.OwnerOfPathIsActualUser(sharedPath, await userManager.GetUserAsync(User))));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while getting files and directories"));

                return View(new SharingDetailsViewModel(new List<CloudFile>(),
                                                        new List<CloudFolder>(),
                                                        pathdata.CurrentPathShow,
                                                        false));
            }
        }

        /// <summary>
        /// Action method to navigate backwards in shared file system
        /// </summary>
        /// <returns>Redirection to details</returns>
        public async Task<IActionResult> Back()
        {
            try
            {
                SharedPathData pathdata = await GetSessionSharedPathData();

                pathdata.RemoveFolderFromPrevDirs();

                await SetSessionSharedPathData(pathdata);

                return RedirectToAction("Details", "Sharing");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while naviating in file system"));

                return RedirectToAction("Details", "Sharing");
            }
        }

        /// <summary>
        /// Action method to navigate to home directory in shared file system
        /// </summary>
        /// <returns>Redirection to details</returns>
        public async Task<IActionResult> Home()
        {
            if (!await SetSessionSharedPathData(new SharedPathData()))
            {
                AddNewNotification(new Error("Error while handling session for shared directory"));
            }

            return RedirectToAction("Details", "Sharing");
        }

        /// <summary>
        /// Action method to handle folder download in shared directories (current state)
        /// </summary>
        /// <param name="folderName">Name of folder</param>
        /// <returns>Redirection to details</returns>
        public async Task<IActionResult> DownloadFolder(string? folderName)
        {
            try
            {
                SharedPathData sharedPath = await GetSessionSharedPathData();

                string cloudPath = await service.ChangePathStructure(sharedPath.CurrentPath);

                if (String.IsNullOrWhiteSpace(folderName))
                {
                    AddNewNotification(new Error("Invalid folder name"));

                    return RedirectToAction("Details", "Sharing");
                }

                SharedFolder sharedFolder = await service.GetSharedFolderByPathAndName(cloudPath, folderName);

                if (sharedFolder.ConnectedToApp == false)
                    throw new Exception("Directory is not shared");

                return await Download(new List<string>()
                {
                    Constants.SelectedFolderStarterSymbol + folderName
                },
                cloudPath,
                RedirectToAction("Details", "Drive"),
                connectedToApp: true);
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while downloading directory"));

                return RedirectToAction("Details", "Sharing");
            }
        }


        /// <summary>
        /// Action method to handle file download in shared directories (current state)
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>Redirection to details</returns>
        public async Task<IActionResult> DownloadFile(string? fileName)
        {
            try
            {
                SharedPathData sharedPath = await GetSessionSharedPathData();

                string cloudPath = await service.ChangePathStructure(sharedPath.CurrentPath);

                if (String.IsNullOrWhiteSpace(fileName))
                {
                    AddNewNotification(new Error("Invalid folder name"));

                    return RedirectToAction("Details", "Sharing");
                }

                SharedFile sharedFile = await service.GetSharedFileByPathAndName(cloudPath, fileName);

                if (sharedFile.ConnectedToApp == false)
                    throw new Exception("File is not shared");

                return await Download(new List<string>()
                {
                    Constants.SelectedFileStarterSymbol + fileName
                },
                cloudPath,
                RedirectToAction("Details", "Drive"),
                connectedToApp: true);
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while downloading file"));

                return RedirectToAction("Details", "Sharing");
            }
        }

        /// <summary>
        /// Action method to show downloadable shared items in current sharing state
        /// </summary>
        /// <returns>View with downloadable items</returns>
        public async Task<IActionResult> DownloadItems()
        {
            SharedPathData pathData = await GetSessionSharedPathData();

            try
            {
                var files = await service.GetCurrentDepthAppSharingFiles(pathData.CurrentPath);
                var folders = await service.GetCurrentDepthAppSharingDirectories(pathData.CurrentPath);

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

        /// <summary>
        /// Action method to handle post request to download items from current sharing state
        /// </summary>
        /// <param name="vm">List of items to download wrapped in DriveDownloadViewModel</param>
        /// <returns>Download action</returns>
        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm([Bind("ItemsForDownload")] DriveDownloadViewModel vm)
        {
            try
            {
                SharedPathData sharedPath = await GetSessionSharedPathData();

                string cloudPath = await service.ChangePathStructure(sharedPath.CurrentPath);

                foreach (var item in vm.ItemsForDownload ?? new())
                {
                    if (item.StartsWith(Constants.SelectedFileStarterSymbol))
                        if (!(await service.GetSharedFileByPathAndName(cloudPath, item[1..])).ConnectedToApp)
                            throw new Exception("File is not shared");

                    if (item.StartsWith(Constants.SelectedFolderStarterSymbol))
                        if (!(await service.GetSharedFolderByPathAndName(cloudPath, item[1..])).ConnectedToApp)
                            throw new Exception("Directory is not shared");
                }

                return await Download(vm.ItemsForDownload ?? new(), await service.ChangePathStructure((await GetSessionSharedPathData()).CurrentPath), RedirectToAction("Details", "Sharing"), connectedToApp: true);

            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while downloading items"));

                return RedirectToAction("Details", "Sharing");
            }
        }

        /// <summary>
        /// Action method to handle folder disconnection from app
        /// </summary>
        /// <param name="itemName">Name of folder</param>
        /// <returns>Json containing a bool value if action was successful and message for action success</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectDirectoryFromApp([FromBody] string itemName)
        {
            try
            {
                SharedPathData pathData = await GetSessionSharedPathData();

                if (await service.DisconnectDirectoryFromApp(await service.ChangePathStructure(pathData.CurrentPath), itemName, await userManager.GetUserAsync(User)))
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
        /// Action method to handle file disconnection from app
        /// </summary>
        /// <param name="itemName">Name of file</param>
        /// <returns>Json containing a bool value if action was successful and message for action success</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectFileFromApp([FromBody] string itemName)
        {
            try
            {
                SharedPathData pathData = await GetSessionSharedPathData();

                if (await service.DisconnectFileFromApp(await service.ChangePathStructure(pathData.CurrentPath), itemName, await userManager.GetUserAsync(User)))
                {
                    return Json(new ConnectionDTO { Success = true, Message = "File disconnected from application" });
                }
                else
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while disconnecting file from application!" });
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
    }
}
