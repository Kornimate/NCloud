using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using System.Drawing.Drawing2D;
using System.Text.Json;
using PathData = NCloud.Models.PathData;

namespace NCloud.Controllers
{
    public class CloudControllerDefault : Controller
    {
        protected readonly ICloudService service;
        protected readonly IWebHostEnvironment env;
        protected readonly INotyfService notifier;
        protected readonly UserManager<CloudUser> userManager;
        protected readonly SignInManager<CloudUser> signInManager;
        protected const string USERCOOKIENAME = "pathDataUser";
        protected const string SHAREDCOOKIENAME = "pathDataShared";
        protected const string APPNAME = "NCloudDrive";
        protected readonly List<string> ALLOWEDFILETYPES = new List<string>();

        protected const string JSONCONTAINERNAME = "__JsonContainer__.json";
        public CloudControllerDefault(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier)
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
        protected PathData GetSessionSharedPathData()
        {
            PathData data = null!;
            if (HttpContext.Session.Keys.Contains(SHAREDCOOKIENAME))
            {
                data = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(SHAREDCOOKIENAME)!)!;
            }
            else
            {
                data = new PathData();
                HttpContext.Session.SetString(SHAREDCOOKIENAME, JsonSerializer.Serialize<PathData>(data));
            }
            return data;
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
        protected void HandleNotifications(List<string>? notifications)
        {
            if (notifications is null) return;
            if (!notifications.Any()) return;
            foreach(var notification in notifications)
            {
                string[] values = notification.Split("--", StringSplitOptions.RemoveEmptyEntries);
                switch (Enum.Parse<NotificationType>(values[1]))
                {
                    case NotificationType.WARNING:
                        notifier.Warning(values[0]);
                        break;
                    case NotificationType.ERROR:
                        notifier.Error(values[0]);
                        break;
                    case NotificationType.INFO:
                        notifier.Information(values[0]);
                        break;
                    case NotificationType.SUCCESS:
                        notifier.Success(values[0]);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
