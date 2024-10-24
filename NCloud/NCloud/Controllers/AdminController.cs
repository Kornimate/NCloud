﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
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

        /// <summary>
        /// Index action to list admin activities
        /// </summary>
        /// <returns>View of page</returns>
        public async Task<IActionResult> Index()
        {
            return await Task.FromResult<IActionResult>(View());
        }

        /// <summary>
        /// Index function to list users for handling
        /// </summary>
        /// <returns>View of page or in case of problem a redirection</returns>
        public async Task<IActionResult> ManageUserAccounts()
        {
            try
            {
                return await Task.FromResult<IActionResult>(View(new AdminUserManagementViewModel(await service.GetCloudUsers())));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Failed to retrieve information about users"));

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Post function to handle accound deletions
        /// </summary>
        /// <param name="Ids">List of Ids in string form</param>
        /// <returns>Redirection to ManageUserAccounts</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserAccounts(List<string> Ids)
        {
            Ids ??= [];

            if (Ids.Count == 0)
            {
                AddNewNotification(new Warning("No user selected for action"));

                return RedirectToAction("ManageUserAccounts");
            }

            int counter = 0;

            CloudUser? user = null;

            foreach (string userId in Ids)
            {
                try
                {
                    user = await service.GetUserById(Guid.Parse(userId));

                    if ((await userManager.GetRolesAsync(user)).Contains(Constants.AdminRole))
                        throw new CloudFunctionStopException($"admin user can not be deleted ({user.UserName})");

                    await service.DeleteDirectoriesForUser(Constants.GetPrivateBaseDirectoryForUser(user.Id.ToString()), user);

                    await userManager.DeleteAsync(user);

                    counter++;
                }
                catch (CloudFunctionStopException ex)
                {
                    AddNewNotification(new Error($"Error - {ex.Message}"));

                    logger.LogError($"Failed to remove user and folders for user: {user?.UserName} {user?.Id.ToString()}");
                }
                catch (Exception)
                {
                    AddNewNotification(new Error($"Error while removing user ({user?.UserName})"));

                    logger.LogError($"Failed to remove user and folders for user: {user?.UserName} {user?.Id.ToString()}");
                }
            }

            if (counter == Ids.Count)
            {
                AddNewNotification(new Success("Account(s) deleted successfully"));
            }
            else
            {
                AddNewNotification(new Warning("Some actions could not be completed"));
            }

            return RedirectToAction("ManageUserAccounts");
        }

        /// <summary>
        /// Post action to handle account disable
        /// </summary>
        /// <param name="Ids">List of ids in string format</param>
        /// <returns>Redirection to ManageUserAccounts</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableUserAccounts(List<string> Ids)
        {
            if (Ids is null || Ids.Count == 0)
            {
                AddNewNotification(new Warning("No user selected for action"));

                return RedirectToAction("ManageUserAccounts");
            }

            int counter = 0;

            CloudUser? user = null;

            foreach (string userId in Ids)
            {
                try
                {
                    user = await service.GetUserById(Guid.Parse(userId));

                    if ((await userManager.GetRolesAsync(user)).Contains(Constants.AdminRole))
                        throw new CloudFunctionStopException($"admin user can not be locked out ({user.UserName})");

                    if (await service.LockOutUser(user))
                    {
                        counter++;
                    }
                    else
                    {
                        throw new CloudFunctionStopException($"user lockout failed {user.UserName}");
                    }

                }
                catch (CloudFunctionStopException ex)
                {
                    AddNewNotification(new Error($"Error - {ex.Message}"));

                    logger.LogError($"Failed to lock user out: {user?.UserName} {user?.Id.ToString()}");
                }
                catch (Exception)
                {
                    AddNewNotification(new Error($"Error while locking user out {user?.UserName}"));

                    logger.LogError($"Failed to lock user out: {user?.UserName} {user?.Id.ToString()}");
                }
            }

            if (counter == Ids.Count)
            {
                AddNewNotification(new Success("User(s) locked out successfully"));
            }
            else
            {
                AddNewNotification(new Warning("Some actions could not be completed"));
            }

            return RedirectToAction("ManageUserAccounts");
        }

        /// <summary>
        /// Post action to handle account enable
        /// </summary>
        /// <param name="Ids">List of ids in string format</param>
        /// <returns>Redirection to ManageUserAccounts</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableUserAccounts(List<string> Ids)
        {
            if (Ids is null || Ids.Count == 0)
            {
                AddNewNotification(new Warning("No user selected for action"));

                return RedirectToAction("ManageUserAccounts");
            }

            int counter = 0;

            CloudUser? user = null;

            foreach (string userId in Ids)
            {
                try
                {
                    user = await service.GetUserById(Guid.Parse(userId));

                    if (await service.EnableUser(user))
                    {
                        counter++;
                    }
                    else
                    {
                        throw new CloudFunctionStopException($"user account reactivation failed {user.UserName}");
                    }

                }
                catch (CloudFunctionStopException ex)
                {
                    AddNewNotification(new Error($"Error - {ex.Message}"));

                    logger.LogError($"Failed to reactive user account: {user?.UserName} {user?.Id.ToString()}");
                }
                catch (Exception)
                {
                    AddNewNotification(new Error($"Error while reactivatin user account {user?.UserName}"));

                    logger.LogError($"Failed to reactive user account: {user?.UserName} {user?.Id.ToString()}");
                }
            }

            if (counter == Ids.Count)
            {
                AddNewNotification(new Success("User account(s) reactivated successfully"));
            }
            else
            {
                AddNewNotification(new Warning("Some actions could not be completed"));
            }

            return RedirectToAction("ManageUserAccounts");
        }

        /// <summary>
        /// Post action to handle user space change
        /// </summary>
        /// <param name="userId">Id of user in GUID format</param>
        /// <param name="newSize">New Size for user in string format</param>
        /// <returns>Redirection to ManageUserAccounts</returns>
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

        /// <summary>
        /// Get action to handle monitoring page
        /// </summary>
        /// <returns>View of monitoring page</returns>
        public async Task<IActionResult> Monitoring()
        {
            try
            {
                return await Task.FromResult<IActionResult>(View(new MonitorViewModel
                {
                    LineGraphPoints = await service.CreateLineGraphPoints(await service.GetLast30DaysLoginsCount()),
                    HeatMapPoints = await service.CreateHeatMapPoints(await service.GetLastWeeksLoginsGroupped())
                }));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Unable to retrieve monitoring data"));

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Get action to list space requests
        /// </summary>
        /// <returns>View of space requests</returns>
        public async Task<IActionResult> ListSpaceRequests()
        {
            try
            {
                return View(await service.GetSpaceRequests());
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Unable to retrieve requests\' list"));

                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Post action to handle space request filfilment
        /// </summary>
        /// <param name="ids">List of ids in GUID format</param>
        /// <returns>Redirection to ListSpaceRequests</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FulfilSpaceRequest(List<Guid>? ids)
        {
            if (ids is null)
            {
                AddNewNotification(new Error("Invalid request"));

                return RedirectToAction("ListSpaceRequests");
            }

            if (ids.Count == 0)
            {
                AddNewNotification(new Warning("No items selected for action"));

                return RedirectToAction("ListSpaceRequests");
            }

            try
            {
                await service.FulfilSpaceRequest(ids);

                AddNewNotification(new Success("Requests handled"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Unable to fulfil request"));
            }

            return RedirectToAction("ListSpaceRequests");
        }

        /// <summary>
        /// Post action to handle space request deletion
        /// </summary>
        /// <param name="ids">List of ids in GUID format</param>
        /// <returns>Redirection to ListSpaceRequests</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpaceRequest(List<Guid>? ids)
        {
            if (ids is null)
            {
                AddNewNotification(new Error("Invalid request"));

                return RedirectToAction("ListSpaceRequests");
            }

            if (ids.Count == 0)
            {
                AddNewNotification(new Warning("No items selected for action"));

                return RedirectToAction("ListSpaceRequests");
            }

            try
            {
                await service.DeleteSpaceRequest(ids);

                AddNewNotification(new Success("Requests handled"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Unable to delete request"));
            }

            return RedirectToAction("ListSpaceRequests");
        }
    }
}
