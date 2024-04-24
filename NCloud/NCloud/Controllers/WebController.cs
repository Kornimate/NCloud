using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models.Extensions;
using NCloud.Security;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    [AllowAnonymous]
    public class WebController : CloudControllerDefault
    {
        public WebController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<IActionResult> SharingPage(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(View("Details", new WebDetailsViewModel(await service.GetCurrentDepthWebFiles(path),
                                                                                     await service.GetCurrentDepthWebDirectories(path),
                                                                                     path)));
        }

        public async Task<IActionResult> Details(string path, string? folderName = null)
        {
            if (folderName is not null && folderName != String.Empty)
            {
                path = Path.Combine(path, folderName);
            }

            return await Task.FromResult<IActionResult>(View("Details", new WebDetailsViewModel(await service.GetCurrentDepthWebFiles(path),
                                                                         await service.GetCurrentDepthWebDirectories(path),
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

        public async Task<IActionResult> DownloadItems(string path)
        {
            return await Task.FromResult<IActionResult>(View());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DownloadItems")]
        public async Task<IActionResult> DownloadItemsFromForm(string path)
        {
            return await Task.FromResult<IActionResult>(View());
        }


        public async Task<IActionResult> DownloadPage(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(View());
        }

        public async Task<IActionResult> Download(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(Content(path));
        }
    }
}
