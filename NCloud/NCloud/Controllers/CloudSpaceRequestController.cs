﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    [Authorize]
    public class CloudSpaceRequestController : CloudControllerDefault
    {
        private EmailTemplateService emailTemplateService;
        public CloudSpaceRequestController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger, IEmailSender emailSender, IConfiguration config) : base(service, userManager, signInManager, env, notifier, logger)
        {
            emailTemplateService = new EmailTemplateService(emailSender, config);
        }

        public async Task<IActionResult> Create()
        {
            CloudUser? user = await userManager.GetUserAsync(User)!;

            if (user is null)
            {
                AddNewNotification(new Error("Can not retrieve user information"));

                return RedirectToAction("UserPage", "UserManagement");
            }

            if (user.MaxSpace == (double)SpaceSizes.GB100)
            {
                AddNewNotification(new Information("Cloud space is already on maximum"));

                return RedirectToAction("UserPage", "UserManagement");
            }

            if (user.CloudSpaceRequest is not null)
            {
                AddNewNotification(new Warning("One request has already been sent"));

                return RedirectToAction("UserPage", "UserManagement");
            }

            return await Task.FromResult<IActionResult>(View(new SpaceRequestViewModel
            {
                User = user
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SpaceRequest,RequestJustification")] SpaceRequestViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    CloudUser? user = await userManager.GetUserAsync(User);

                    await service.CreateNewSpaceRequest(new CloudSpaceRequest
                    {
                        SpaceRequest = Enum.Parse<SpaceSizes>(vm.SpaceRequest),
                        RequestJustification = vm.RequestJustification
                    }, user);

                    AddNewNotification(new Success("Request has been successfully sent"));

                    await emailTemplateService.SendEmailAsync(new CloudUserSpaceRequest(emailTemplateService.GetSelfEmailAddress(), $"{user?.UserName} created a new cloud space request!"));

                    return RedirectToAction("UserPage", "UserManagement");
                }
                catch (CloudFunctionStopException ex)
                {
                    AddNewNotification(new Error(ex.Message));
                }
                catch (Exception)
                {
                    AddNewNotification(new Error("Unexpected error while submitting request"));
                }

                return RedirectToAction("Create");
            }

            AddNewNotification(new Error("Invalid data in submitted form"));

            return RedirectToAction("Create");
        }
    }
}
