﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle web sharing requests
    /// </summary>
    [AllowAnonymous]
    public class WebController : CloudControllerDefault
    {
        public WebController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        /// <summary>
        /// Action method to decrypt path to folder (web shared) and show items in it
        /// </summary>
        /// <param name="id">Encrypted id of folder</param>
        /// <returns>View with files and folders at the current state (web shared)</returns>
        public async Task<IActionResult> SharingPage(string id)
        {
            try
            {
                Guid folderId = Guid.Parse(id);

                SharedFolder sharedFolder = await service.GetWebSharedFolderById(folderId);

                if (sharedFolder.ConnectedToWeb == false)
                    throw new Exception("Directory is not shared");

                string path = Path.Combine(sharedFolder.CloudPathFromRoot, sharedFolder.Name);

                return await Task.FromResult<IActionResult>(View("Details", new WebDetailsViewModel(await service.GetCurrentDepthWebSharingFiles(path),
                                                                                                    await service.GetCurrentDepthWebSharingDirectories(path),
                                                                                                    path)));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while getting files and directories"));

                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action method to handle navigation inside shared file system
        /// </summary>
        /// <param name="path">Path to current state</param>
        /// <param name="folderName">Name of folder</param>
        /// <returns>View with items of specified folder</returns>
        public async Task<IActionResult> Details(string path, string? folderName = null)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(folderName))
                {
                    path = Path.Combine(path, folderName);
                }

                return await Task.FromResult<IActionResult>(View("Details", new WebDetailsViewModel(await service.GetCurrentDepthWebSharingFiles(path),
                                                                                                    await service.GetCurrentDepthWebSharingDirectories(path),
                                                                                                    path)));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while getting files and directories"));

                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action method to handle backwards navigation in web shared file system
        /// </summary>
        /// <param name="path">Path to current state</param>
        /// <returns></returns>
        public async Task<IActionResult> Back(string path)
        {
            if (!String.IsNullOrWhiteSpace(path))
            {
                path = await service.WebBackCheck(path);
            }

            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Details), new { path = path }));
        }

        /// <summary>
        /// Action methdo to handle download of web shared folder
        /// </summary>
        /// <param name="path">Path to folder</param>
        /// <param name="folderName">Name of folder</param>
        /// <returns>Redirect to download</returns>
        public async Task<IActionResult> DownloadFolder(string? path, string? folderName)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(folderName) || String.IsNullOrWhiteSpace(path))
                {
                    return RedirectToAction("Error", "Home");
                }

                SharedFolder sharedFolder = await service.GetSharedFolderByPathAndName(path, folderName);

                if (sharedFolder.ConnectedToWeb == false)
                    throw new Exception("Directory is not shared");

                return await Download(new List<string>()
                {
                    Constants.SelectedFolderStarterSymbol + folderName
                },
                path,
                RedirectToAction("Details", "Web", new { path = path }),
                connectedToWeb: true);
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Directory is not shared on this URL"));

                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action methdo to handle download of web shared file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="fileName">Name of file</param>
        /// <returns>Redirect to download</returns>
        public async Task<IActionResult> DownloadFile(string? path, string? fileName)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(fileName) || String.IsNullOrWhiteSpace(path))
                {
                    return RedirectToAction("Error", "Home");
                }

                SharedFile sharedFile = await service.GetSharedFileByPathAndName(path, fileName);

                if (sharedFile.ConnectedToWeb == false)
                    throw new Exception("File is not shared");

                return await Download(new List<string>()
                {
                    Constants.SelectedFileStarterSymbol + fileName
                },
                path,
                RedirectToAction("Details", "Web", new { path = path }),
                connectedToWeb: true);
            }
            catch (Exception)
            {
                AddNewNotification(new Error("File is not shared on this URL"));

                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action method to handle download items form current web sharing state
        /// </summary>
        /// <param name="path">Path to current state</param>
        /// <returns>View with downloadable elements</returns>
        public async Task<IActionResult> DownloadItems(string path)
        {
            try
            {
                var files = await service.GetCurrentDepthWebSharingFiles(path);
                var folders = await service.GetCurrentDepthWebSharingDirectories(path);

                return View(new WebDownloadViewModel
                {
                    Folders = folders,
                    Files = files,
                    ItemsForDownload = new string[files.Count + folders.Count].ToList(),
                    Path = path
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while downloading items"));

                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action method to handle post request to download items from current web sharing state
        /// </summary>
        /// <param name="vm">Items for download wrapped in WebDownloadViewModel</param>
        /// <returns>Redirect to download</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm(WebDownloadViewModel vm)
        {
            try
            {
                foreach (var item in vm.ItemsForDownload ?? new())
                {
                    if (item.StartsWith(Constants.SelectedFileStarterSymbol))
                        if (!(await service.GetSharedFileByPathAndName(vm.Path, item[1..])).ConnectedToWeb)
                            throw new Exception("File is not shared");

                    if (item.StartsWith(Constants.SelectedFolderStarterSymbol))
                        if (!(await service.GetSharedFolderByPathAndName(vm.Path, item[1..])).ConnectedToWeb)
                            throw new Exception("Directory is not shared");
                }

                return await Download(vm.ItemsForDownload ?? new(), vm.Path, RedirectToAction("Details", "Web", new { path = vm.Path }), connectedToWeb: true);
            }
            catch (Exception)
            {
                AddNewNotification(new Error("One or more files not shared on this URL"));

                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action method to handle single file download
        /// </summary>
        /// <param name="path">Path to current state</param>
        /// <returns>View with downloadable file in it</returns>
        public async Task<IActionResult> DownloadPage(string id)
        {
            try
            {
                Guid fileId = Guid.Parse(id);

                SharedFile sharedFile = await service.GetWebSharedFileById(fileId);

                if (sharedFile.ConnectedToWeb == false)
                    throw new Exception("File is not shared");

                return await Task.FromResult<IActionResult>(View("DownloadPage", new WebSingleDownloadViewModel()
                {
                    FilePath = sharedFile.CloudPathFromRoot,
                    FileName = sharedFile.Name
                }));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("File is not shared on this URL"));

                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadSingleItem([Bind("FileName,FilePath")] WebSingleDownloadViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    SharedFile sharedFile = await service.GetSharedFileByPathAndName(vm.FilePath, vm.FileName);

                    if (sharedFile.ConnectedToWeb == false)
                        throw new Exception("File is not shared");

                    return await Download(new List<string>() { Constants.SelectedFileStarterSymbol + vm.FileName }, vm.FilePath, RedirectToAction("Error", "Home"), connectedToWeb: true);
                }

                AddNewNotification(new Error("Invalid download data"));

                return View(vm);
            }
            catch (Exception)
            {
                AddNewNotification(new Error("File is not shared on this URL"));

                return RedirectToAction("Error", "Home");
            }
        }
    }
}
