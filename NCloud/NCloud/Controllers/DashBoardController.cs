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
        public DashBoardController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        public async Task<ActionResult> Index()
        {
            CloudUser user = await userManager.GetUserAsync(User);

            double usedPercent = Math.Ceiling(user.UsedSpace / user.MaxSpace);

            if (usedPercent < 0.0)
                usedPercent = 0.0;

            if(usedPercent > 100.0)
                usedPercent = 100.0;

            return View(new DashBoardViewModel(await service.GetUserSharedFolderUrls(User), await service.GetUserSharedFileUrls(User), Constants.GetWebControllerAndActionForDetails(), Constants.GetWebControllerAndActionForDownload(), usedPercent));
        }
    }
}
