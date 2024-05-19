using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle dashboard requests
    /// </summary>
    [Authorize]
    public class DashBoardController : CloudControllerDefault
    {
        public DashBoardController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        /// <summary>
        /// Action method to show user details and web shared files and folders
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index()
        {
            try
            {
                CloudUser user = await userManager.GetUserAsync(User);

                await service.CheckUserStorageUsed(user);

                double usedPercent = Math.Ceiling((user.UsedSpace / user.MaxSpace)*100);

                if (usedPercent < 0.0)
                    usedPercent = 0.0;

                if (usedPercent > 100.0)
                    usedPercent = 100.0;

                return View(new DashBoardViewModel(await service.GetUserSharedFolderUrls(user), await service.GetUserSharedFileUrls(user), Constants.GetWebControllerAndActionForDetails(), Constants.GetWebControllerAndActionForDownload(), usedPercent, user.UsedSpace));

            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));

                return View(new DashBoardViewModel(new List<string>(), new List<string>(), Constants.GetWebControllerAndActionForDetails(), Constants.GetWebControllerAndActionForDownload(), 0, 0.0));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while loading page"));

                return View(new DashBoardViewModel(new List<string>(), new List<string>(), Constants.GetWebControllerAndActionForDetails(), Constants.GetWebControllerAndActionForDownload(), 0, 0.0));
            }
        }
    }
}
