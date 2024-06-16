using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services.Exceptions;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle actions only possible for admin users
    /// </summary>

    [Authorize(Roles = Constants.AdminRoleName)]
    public class AdminController : CloudControllerDefault
    {
        private IEmailSender emailService;
        public AdminController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger, IEmailSender emailService) : base(service, userManager, signInManager, env, notifier, logger)
        {
            this.emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            return await Task.FromResult<IActionResult>(View());
        }

        public async Task<IActionResult> ManageUserAccounts()
        {
            return await Task.FromResult<IActionResult>(View(new AdminUserManagementViewModel(await service.GetCloudUsers())));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserAccounts(List<string> userIds)
        {
            //TODO: implement method

            return RedirectToAction("ManageUserAccounts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableUserAccounts()
        {
            //TODO: implement method

            return RedirectToAction("ManageUserAccounts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableUserAccounts()
        {
            //TODO: implement method

            return RedirectToAction("ManageUserAccounts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserMaxSpace(Guid userId, string newSize)
        {
            bool successfulParsing = Enum.TryParse<SpaceSizes>(newSize, out SpaceSizes spaceSize);

            if (!successfulParsing)
            {
                AddNewNotification(new Error("Invalid space size"));

                return RedirectToAction("ManageUserAccounts");
            }

            try
            {
                await service.SetUserSpaceSize(userId, spaceSize);

                AddNewNotification(new Success("User space size adjusted"));
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error($"Error while adjusting user space size"));
            }

            return RedirectToAction("ManageUserAccounts");
        }

        public async Task<IActionResult> Monitoring()
        {
            return await Task.FromResult<IActionResult>(View());
        }
    }
}
