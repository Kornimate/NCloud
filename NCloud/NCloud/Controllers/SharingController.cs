using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.DTOs;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.IO.Compression;
using System.Text.Json;

namespace NCloud.Controllers
{
    [Authorize]
    public class SharingController : CloudControllerDefault
    {
        public SharingController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        public async Task<IActionResult> Details(string? folderName = null)
        {
            SharedPathData pathdata = await GetSessionSharedPathData();

            string sharedPath = pathdata.SetFolder(folderName);

            await SetSessionSharedPathData(pathdata);

            try
            {
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

        public async Task<IActionResult> Back()
        {
            SharedPathData pathdata = await GetSessionSharedPathData();

            pathdata.RemoveFolderFromPrevDirs();

            await SetSessionSharedPathData(pathdata);

            return RedirectToAction("Details", "Sharing");
        }

        public async Task<IActionResult> Home()
        {
            await SetSessionSharedPathData(new SharedPathData());

            return RedirectToAction("Details", "Sharing");
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
            await service.ChangePathStructure((await GetSessionSharedPathData()).CurrentPath),
            RedirectToAction("Details", "Drive"),
            connectedToApp: true);
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
            await service.ChangePathStructure((await GetSessionSharedPathData()).CurrentPath),
            RedirectToAction("Details", "Drive"),
            connectedToApp: true);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken, ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm([Bind("ItemsForDownload")] DriveDownloadViewModel vm)
        {
            return await Download(vm.ItemsForDownload ?? new(), await service.ChangePathStructure((await GetSessionSharedPathData()).CurrentPath), RedirectToAction("Details", "Sharing"), connectedToApp: true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisconnectDirectoryFromApp([FromBody] string itemName)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DisconnectFileFromApp([FromBody] string itemName)
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
    }
}
