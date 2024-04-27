using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Models.Extensions;
using NCloud.Security;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.Drawing.Drawing2D;
using System.IO;

namespace NCloud.Controllers
{
    [AllowAnonymous]
    public class WebController : CloudControllerDefault
    {
        public WebController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<IActionResult> SharingPage(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(View("Details", new WebDetailsViewModel(await service.GetCurrentDepthWebSharingFiles(path),
                                                                                     await service.GetCurrentDepthWebSharingDirectories(path),
                                                                                     path)));
        }

        public async Task<IActionResult> Details(string path, string? folderName = null)
        {
            if (folderName is not null && folderName != String.Empty)
            {
                path = Path.Combine(path, folderName);
            }

            return await Task.FromResult<IActionResult>(View("Details", new WebDetailsViewModel(await service.GetCurrentDepthWebSharingFiles(path),
                                                                         await service.GetCurrentDepthWebSharingDirectories(path),
                                                                         path)));
        }
        public async Task<IActionResult> Back(string path)
        {
            if (path is not null && path != String.Empty)
            {
                path = await service.WebBackCheck(path);
            }

            return await Task.FromResult<IActionResult>(RedirectToAction(nameof(Details), new { path = path }));
        }

        public async Task<IActionResult> DownloadFolder(string? path, string? folderName)
        {
            if (folderName is null || folderName == String.Empty || path is null || path == String.Empty)
            {
                return View("Error");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFolderStarterSymbol + folderName
            },
            path,
            RedirectToAction("Details", "Web", new { path = path }),
            connectedToWeb: true);
        }

        public async Task<IActionResult> DownloadFile(string? path, string? fileName)
        {
            if (fileName is null || fileName == String.Empty || path is null || path == String.Empty)
            {
                return View("Error");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFileStarterSymbol + fileName
            },
            path,
            RedirectToAction("Details", "Web", new { path = path }),
            connectedToWeb: true);
        }

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
            catch (Exception ex)
            {
                AddNewNotification(new Error(ex.Message));
                return View(new WebDownloadViewModel
                {
                    Folders = new List<CloudFolder>(),
                    Files = new List<CloudFile>(),
                    ItemsForDownload = Array.Empty<string>().ToList(),
                    Path = path
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm(WebDownloadViewModel vm)
        {
            return await Download(vm.ItemsForDownload ?? new(), vm.Path, RedirectToAction("Details", "Web", new { path = vm.Path }), connectedToWeb: true);
        }


        public async Task<IActionResult> DownloadPage(string path)
        {
            path = HashManager.DecryptString(path);

            string fileName = Path.GetFileName(path);

            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));

            return await Task.FromResult<IActionResult>(View("DownloadPage", new WebSingleDownloadViewModel()
            {
                Path = path,
                FileName = fileName
            }));
        }

        public async Task<IActionResult> DownloadSingleItem(string path, string fileName)
        {
            return await Download(new List<string>() { Constants.SelectedFileStarterSymbol + fileName }, path, RedirectToAction("DownloadPage", new { path = HashManager.EncryptString(Path.Combine(path, fileName)) }), connectedToWeb: true);
        }
    }
}
