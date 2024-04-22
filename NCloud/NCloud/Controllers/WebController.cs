using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Details(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(View(new WebDetailsViewModel(await service.GetCurrentDepthCloudFiles(path, User),
                                                                                     await service.GetCurrentDepthCloudDirectories(path, User),
                                                                                     path,
                                                                                     path)));
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

        public async Task<IActionResult> Back(string path)
        {
            return null!;
        }

        public async Task<IActionResult> DownloadPage(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(View());
        }

        public async Task<IActionResult> Download (string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(Content(path));
        }
    }
}
