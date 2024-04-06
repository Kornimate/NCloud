using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    public class TerminalController : CloudControllerDefault
    {
        public TerminalController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<IActionResult> Index(string? currentPath = null)
        {
            currentPath ??= (await GetSessionUserPathData()).CurrentPathShow;
            return View(new TerminalViewModel
            {
                CurrentDirectory = currentPath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Evaluate([FromBody]string command)
        {
            if (command is null)
                return BadRequest();

            return await Task.FromResult<IActionResult>(Content("Success"));
        }
    }
}
