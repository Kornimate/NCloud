using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NuGet.Protocol;
using System.Drawing.Drawing2D;
using System.Text.Json;
using CloudPathData = NCloud.Models.CloudPathData;

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
        protected CloudPathData GetSessionUserPathData()
        {
            CloudPathData data = null!;
            if (HttpContext.Session.Keys.Contains(USERCOOKIENAME))
            {
                data = JsonSerializer.Deserialize<CloudPathData>(HttpContext.Session.GetString(USERCOOKIENAME)!)!;
            }
            else
            {
                data = new CloudPathData();
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
        protected void SetSessionUserPathData(CloudPathData pathData)
        {
            if (pathData == null) return;
            HttpContext.Session.SetString(USERCOOKIENAME, JsonSerializer.Serialize<CloudPathData>(pathData));
        }

        [NonAction]
        protected SharedPathData GetSessionSharedPathData()
        {
            SharedPathData data = null!;
            if (HttpContext.Session.Keys.Contains(SHAREDCOOKIENAME))
            {
                data = JsonSerializer.Deserialize<SharedPathData>(HttpContext.Session.GetString(SHAREDCOOKIENAME)!)!;
            }
            else
            {
                data = new SharedPathData();
                HttpContext.Session.SetString(SHAREDCOOKIENAME, JsonSerializer.Serialize<SharedPathData>(data));
            }
            return data;
        }

        [NonAction]
        protected void SetSessionSharedPathData(SharedPathData pathData) //make it thread-safe
        {
            if (pathData == null) return;
            HttpContext.Session.SetString(SHAREDCOOKIENAME, JsonSerializer.Serialize<SharedPathData>(pathData));
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
        protected void AddNewNotification(CloudNotificationAbstarct notification)
        {
            notifier.AddNotification(notification);
            TempData[NOTIFICATIONKEY] = notifier.GetNotificationQueue(); 
        }
    }
}
