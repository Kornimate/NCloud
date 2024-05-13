using Castle.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NuGet.Protocol;
using System.Drawing.Drawing2D;
using System.IO.Compression;
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
        protected readonly ILogger<CloudControllerDefault> logger;
        public CloudControllerDefault(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger)
        {
            this.service = service;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
            this.notifier = notifier;
            this.logger = logger;
        }

        [NonAction]
        protected async Task<CloudPathData> GetSessionCloudPathData()
        {
            return await service.GetSessionCloudPathData();
        }

        [NonAction]
        protected async Task<bool> SetSessionCloudPathData(CloudPathData pathData)
        {
            return await service.SetSessionCloudPathData(pathData);
        }

        [NonAction]
        protected async Task<SharedPathData> GetSessionSharedPathData()
        {
            return await Task.Run(() =>
            {
                SharedPathData data = null!;
                if (HttpContext.Session.Keys.Contains(Constants.SharedCookieKey))
                {
                    data = JsonSerializer.Deserialize<SharedPathData>(HttpContext.Session.GetString(Constants.SharedCookieKey)!)!;
                }
                else
                {
                    data = new SharedPathData();
                    HttpContext.Session.SetString(Constants.SharedCookieKey, JsonSerializer.Serialize<SharedPathData>(data));
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
                    HttpContext.Session.SetString(Constants.SharedCookieKey, JsonSerializer.Serialize<SharedPathData>(pathData));

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
                TempData[Constants.NotificationCookieKey] = notifier.GetNotificationQueue();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IActionResult> Download(List<string> itemsForDownload, string path, IActionResult returnAction, bool connectedToApp = false, bool connectedToWeb = false)
        {
            try
            {
                if (itemsForDownload is not null && itemsForDownload.Count != 0)
                {
                    if (itemsForDownload.Count > 1 || itemsForDownload[0].StartsWith(Constants.SelectedFolderStarterSymbol))
                    {
                        string? tempFile = null;

                        try
                        {
                            tempFile = await service.CreateZipFile(itemsForDownload, path, GetTempFileNameAndPath(), connectedToApp, connectedToWeb);
                        }
                        catch (Exception ex)
                        {
                            AddNewNotification(new Error(ex.Message));
                        }
                        try
                        {
                            if (tempFile is null)
                                throw new Exception("File was not created");

                            FileStream stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);

                            return File(stream, Constants.ZipMimeType, $"{Constants.AppName}_{DateTime.Now.ToString(Constants.DateTimeFormat)}.{Constants.CompressedArchiveFileType}");
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else
                    {
                        try
                        {
                            if (itemsForDownload[0].StartsWith(Constants.SelectedFileStarterSymbol))
                            {
                                string name = itemsForDownload[0][1..];

                                try
                                {
                                    FileStream stream = new FileStream(Path.Combine(service.ServerPath(path), name), FileMode.Open, FileAccess.Read, FileShare.Read);

                                    return File(stream, MimeTypeManager.GetMimeType(name), name);
                                }
                                catch (Exception)
                                {
                                    throw;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            AddNewNotification(new Error($"Invalid filename: {itemsForDownload[0]}"));
                        }
                    }
                }

                AddNewNotification(new Warning($"No file(s) or folder(s) were chosen for download"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error($"App unable to create file for download"));
            }

            return returnAction;
        }

        [NonAction]
        protected string GetTempFileNameAndPath()
        {
            if (!Directory.Exists(Constants.TempFilePath))
            {
                Directory.CreateDirectory(Constants.TempFilePath);
            }

            return Path.Combine(Constants.TempFilePath, Guid.NewGuid().ToString() + DateTime.Now.ToString(Constants.DateTimeFormat));
        }
    }
}
