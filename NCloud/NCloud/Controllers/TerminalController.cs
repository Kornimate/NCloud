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

        public IActionResult Index(string? currentPath = null)
        {
            currentPath ??= GetSessionUserPathData().CurrentPathShow;
            return View(new TerminalViewModel
            {
                CurrentDirectory = currentPath
            });
        }

        public IActionResult EvaluateSingleLine(string? commandLine)
        {
            return Content("Success");
        }
    }
}
