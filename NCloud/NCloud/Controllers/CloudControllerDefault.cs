﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NuGet.Protocol;
using System.Drawing.Drawing2D;
using System.Text.Json;
using PathData = NCloud.Models.PathData;

namespace NCloud.Controllers
{
    public class CloudControllerDefault : Controller
    {
        protected readonly ICloudService service;
        protected readonly IWebHostEnvironment env;
        protected readonly ICloudNotificationService notifier;
        protected readonly UserManager<CloudUser> userManager;
        protected readonly SignInManager<CloudUser> signInManager;
        protected const string USERCOOKIENAME = "pathDataUser";
        protected const string SHAREDCOOKIENAME = "pathDataShared";
        protected const string APPNAME = "NCloudDrive";
        protected const string NOTIFICATIONKEY = "Notification";
        protected readonly List<string> ALLOWEDFILETYPES = new List<string>(); //TODO: add filetypes

        protected const string JSONCONTAINERNAME = "__JsonContainer__.json";
        public CloudControllerDefault(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier)
        {
            this.service = service;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
            this.notifier = notifier;
        }

        [NonAction]
        protected PathData GetSessionUserPathData()
        {
            PathData data = null!;
            if (HttpContext.Session.Keys.Contains(USERCOOKIENAME))
            {
                data = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(USERCOOKIENAME)!)!;
            }
            else
            {
                data = new PathData();
                CloudUser? user = null;
                Task.Run(async () =>
                {
                    user = await userManager.GetUserAsync(User);
                }).Wait();
                data.SetDefaultPathData(user?.Id?.ToString());
                SetSessionUserPathData(data);
            }
            return data;
        }

        [NonAction]
        protected void SetSessionUserPathData(PathData pathData)
        {
            if (pathData == null) return;
            HttpContext.Session.SetString(USERCOOKIENAME, JsonSerializer.Serialize<PathData>(pathData));
        }

        [NonAction]
        protected SharedData GetSessionSharedPathData()
        {
            SharedData data = null!;
            if (HttpContext.Session.Keys.Contains(SHAREDCOOKIENAME))
            {
                data = JsonSerializer.Deserialize<SharedData>(HttpContext.Session.GetString(SHAREDCOOKIENAME)!)!;
            }
            else
            {
                data = new SharedData();
                HttpContext.Session.SetString(SHAREDCOOKIENAME, JsonSerializer.Serialize<SharedData>(data));
            }
            return data;
        }

        [NonAction]
        protected void SetSessionSharedPathData(SharedData pathData) //make it thread-safe
        {
            if (pathData == null) return;
            HttpContext.Session.SetString(SHAREDCOOKIENAME, JsonSerializer.Serialize<SharedData>(pathData));
        }

        [NonAction]
        protected IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        [NonAction]
        protected void AddNewNotification(ACloudNotification notification)
        {
            notifier.AddNotification(notification);
            TempData[NOTIFICATIONKEY] = notifier.GetNotificationQueue(); 
        }
    }
}