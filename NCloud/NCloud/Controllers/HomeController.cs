using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.Diagnostics;

namespace NCloud.Controllers
{
    public class HomeController : CloudControllerDefault
    {
        public HomeController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<IActionResult> Index()
        {
            return await Task.FromResult<IActionResult>(View());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            return await Task.FromResult<IActionResult>(View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }));
        }
    }
}