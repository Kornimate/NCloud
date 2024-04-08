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
        protected async Task<CloudPathData> GetSessionCloudPathData()
        {
            CloudPathData data = null!;
            if (HttpContext.Session.Keys.Contains(USERCOOKIENAME))
            {
                data = JsonSerializer.Deserialize<CloudPathData>(HttpContext.Session.GetString(USERCOOKIENAME)!)!;
            }
            else
            {
                CloudUser? user = await userManager.GetUserAsync(User);
                data = new CloudPathData();
                data.SetDefaultPathData(user?.Id.ToString());
                await SetSessionCloudPathData(data);
            }
            return data;
        }

        [NonAction]
        protected Task SetSessionCloudPathData(CloudPathData pathData)
        {
            return Task.Run(() =>
            {
                if (pathData == null) return;
                HttpContext.Session.SetString(USERCOOKIENAME, JsonSerializer.Serialize<CloudPathData>(pathData));
            });
        }

        [NonAction]
        protected async Task<SharedPathData> GetSessionSharedPathData()
        {
            return await Task.Run(() =>
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
            });
        }

        [NonAction]
        protected Task<bool> SetSessionSharedPathData(SharedPathData pathData) //make it thread-safe
        {
            return Task.Run(() =>
            {
                try
                {
                    pathData ??= new SharedPathData();
                    HttpContext.Session.SetString(SHAREDCOOKIENAME, JsonSerializer.Serialize<SharedPathData>(pathData));

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        [NonAction]
        protected async Task<IActionResult> RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return await Task.FromResult<IActionResult>(Redirect(returnUrl));
            }
            else
            {
                return await Task.FromResult<IActionResult>(RedirectToAction(nameof(HomeController.Index), "Home"));
            }
        }

        [NonAction]
        protected bool AddNewNotification(CloudNotificationAbstarct notification)
        {
            try
            {
                notifier.AddNotification(notification);
                TempData[NOTIFICATIONKEY] = notifier.GetNotificationQueue();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
