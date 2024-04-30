using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
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

            var result = service.GetCurrentUserIndexData(); //TODO: implement function

            return View(new DashBoardViewModel(result.Item1, result.Item2, await service.GetUserSharedFolderUrls(User), await service.GetUserSharedFileUrls(User), Constants.GetWebControllerAndActionForDetails(), Constants.GetWebControllerAndActionForDownload()));
        }
    }
}
