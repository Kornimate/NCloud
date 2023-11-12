using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    public class DashBoardController : CloudControllerDefault
    {
        public DashBoardController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<ActionResult> Index()
        {
            await signInManager.PasswordSignInAsync("Admin", "Admin_1234", true, false);
            var result = service.GetCurrentUserIndexData();
            AddNewNotification(new Information("Service is working!"));
            return View(new DriveIndexViewModel(result.Item1, result.Item2)
            {
                //TestString = Url.Action("Index", "Drive",new { path="testpath" },Request.Scheme)
            });
        }
    }
}
