using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    [Authorize]
    public class DashBoardController : CloudControllerDefault
    {
        public DashBoardController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<ActionResult> Index()
        {
            CloudUser user = await userManager.GetUserAsync(User);

            double usedPercent = Math.Ceiling(user.UsedSpace / user.MaxSpace);

            return View(new DashBoardViewModel(await service.GetUserSharedFolderUrls(User), await service.GetUserSharedFileUrls(User), Constants.GetWebControllerAndActionForDetails(), Constants.GetWebControllerAndActionForDownload(), usedPercent));
        }
    }
}
