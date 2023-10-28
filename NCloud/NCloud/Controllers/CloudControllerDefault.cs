using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using System.Text.Json;

namespace NCloud.Controllers
{
    public class CloudControllerDefault : Controller
    {
        protected readonly ICloudService service;
        protected readonly IWebHostEnvironment env;
        protected readonly INotyfService notifier;
        protected readonly UserManager<CloudUser> userManager;
        protected readonly SignInManager<CloudUser> signInManager;
        protected const string FOLDERSEPARATOR = "//";
        protected const string COOKIENAME = "pathData";
        protected const string ROOTNAME = "@CLOUDROOT";
        protected const string APPNAME = "NCloudDrive";
        protected readonly List<string> ALLOWEDFILETYPES = new List<string>();
        public CloudControllerDefault(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier)
        {
            this.service = service;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
            this.notifier = notifier;
        }

        [NonAction]
        protected PathData GetSessionPathData()
        {
            PathData data = null!;
            if (HttpContext.Session.Keys.Contains(COOKIENAME))
            {
                data = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
            }
            else
            {
                data = new PathData();
                HttpContext.Session.SetString(COOKIENAME, JsonSerializer.Serialize<PathData>(data));
            }
            return data;
        }

        [NonAction]
        protected void CreateBaseDirectory(CloudUser cloudUser)
        {
            string publicFolder = Path.Combine(env.WebRootPath, "CloudData", "Public");
            if (!Directory.Exists(publicFolder))
            {
                Directory.CreateDirectory(publicFolder);
            }
            string userFolderPath = Path.Combine(env.WebRootPath, "CloudData", "UserData", cloudUser.Id);
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
            }
            Directory.CreateDirectory(Path.Combine(userFolderPath, "Documents"));
            Directory.CreateDirectory(Path.Combine(userFolderPath, "Pictures"));
            Directory.CreateDirectory(Path.Combine(userFolderPath, "Videos"));
            Directory.CreateDirectory(Path.Combine(userFolderPath, "Music"));
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
    }
}
