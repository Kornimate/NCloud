using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.IO.Compression;
using System.Text.Json;

namespace NCloud.Controllers
{
    public class SharingController : CloudControllerDefault
    {
        public SharingController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }
        
        public async Task<IActionResult> Details(string? folderName = null)
        {
            SharedPathData pathdata = await GetSessionSharedPathData();
            
            string currentPath = pathdata.SetFolder(folderName);
            
            await SetSessionSharedPathData(pathdata);
            
            if(pathdata.CurrentPath == Constants.PublicRootName)
            {
                return View(new SharingDetailsViewModel(new List<CloudFile>(),
                                                        await service.GetSharingUsersSharingDirectories(currentPath),
                                                        pathdata.CurrentPathShow,
                                                        false));
            }
            
            try
            {
                return View(new SharingDetailsViewModel(await service.GetCurrentDepthSharingFiles(currentPath,User),
                                                        await service.GetCurrentDepthSharingDirectories(currentPath,User),
                                                        pathdata.CurrentPathShow,
                                                        await service.OwnerOfPathIsActualUser(currentPath,User)));
            }
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));

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

        public async Task<IActionResult> DownloadItems()
        {
            SharedPathData pathData = await GetSessionSharedPathData();
            
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
            return await Download(vm.ItemsForDownload ?? new(), (await GetSessionSharedPathData()).CurrentPath);
        }
    }
}
