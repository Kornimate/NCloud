using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.Diagnostics;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle first page and error
    /// </summary>
    [AllowAnonymous]
    public class HomeController : CloudControllerDefault
    {
        public HomeController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        /// <summary>
        /// Action method to open home page
        /// </summary>
        /// <returns>Home page view</returns>
        public async Task<IActionResult> Index()
        {
            return await Task.FromResult<IActionResult>(View());
        }

        //Need to be removed
        public async Task<IActionResult> TestLogin()
        {
            await signInManager.PasswordSignInAsync("Admin", "Admin_1234", true, false);

            return RedirectToAction("Index", "Dashboard");
        }

        /// <summary>
        /// Action method to handle errors
        /// </summary>
        /// <returns>Error view</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            return await Task.FromResult<IActionResult>(View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }));
        }
    }
}