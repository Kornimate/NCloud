using Castle.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NuGet.Protocol;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using System.Net.Http;
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

        /// <summary>
        /// Non action method to get session data related to cloud navigation and cloud clipboard
        /// </summary>
        /// <returns>The CloudPathData class with information in it</returns>
        [NonAction]
        protected async Task<CloudPathData> GetSessionCloudPathData()
        {
            try
            {
                CloudPathData data = null!;

                if (HttpContext.Session.Keys.Contains(Constants.CloudCookieKey))
                {
                    data = JsonSerializer.Deserialize<CloudPathData>(HttpContext.Session.GetString(Constants.CloudCookieKey)!)!;
                }
                else
                {
                    CloudUser? user = await userManager.GetUserAsync(HttpContext.User);

                    data = new CloudPathData();

                    data.SetDefaultPathData(user?.Id.ToString());

                    await SetSessionCloudPathData(data);
                }

                return data;
            }
            catch (Exception)
            {
                return new CloudPathData();
            }
        }

        /// <summary>
        /// Non action method to save items into session
        /// </summary>
        /// <param name="pathData">Item to be saved to session</param>
        /// <returns>Boolean indicating the success of action</returns>
        [NonAction]
        protected async Task<bool> SetSessionCloudPathData(CloudPathData pathData)
        {
            try
            {
                if (pathData == null)
                    return await Task.FromResult<bool>(false);

                HttpContext.Session.SetString(Constants.CloudCookieKey, JsonSerializer.Serialize<CloudPathData>(pathData));

                return await Task.FromResult<bool>(true);
            }
            catch (Exception)
            {
                return await Task.FromResult<bool>(true);
            }
        }

        /// <summary>
        /// Non action method to get session data related to sharing navigation
        /// </summary>
        /// <returns>The SharedPathData class with information in it</returns>
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

        /// <summary>
        /// Non action method to save items into session
        /// </summary>
        /// <param name="pathData">Item to be saved to session</param>
        /// <returns>Boolean indicating the success of action</returns>
        [NonAction]
        protected Task<bool> SetSessionSharedPathData(SharedPathData pathData)
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


        /// <summary>
        /// Non action method to safe redirection inside app
        /// </summary>
        /// <param name="returnUrl">url to be returned to</param>
        /// <returns>Redirection to url or if not local to home</returns>
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

        /// <summary>
        /// Method to handle notifications presented to user
        /// </summary>
        /// <param name="notification">The notification with correct type (strategy design pattern)</param>
        /// <returns>Boolean indication the success of action</returns>
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

        /// <summary>
        /// Action method to download file or zip
        /// </summary>
        /// <param name="itemsForDownload">List of item names to be downloaded</param>
        /// <param name="path">path to items to be downloaded (same path for every item)</param>
        /// <param name="returnAction">If download fails where to return</param>
        /// <param name="connectedToApp">Filter app shared items</param>
        /// <param name="connectedToWeb">Filter web shared items</param>
        /// <returns>The file (zip or single file)</returns>
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
                        catch(Exception)
                        {
                            throw new CloudFunctionStopException("Error while creating zip file");
                        }

                        try
                        {
                            if (tempFile is null)
                                throw new CloudFunctionStopException("File was not created");

                            FileStream stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);

                            return File(stream, Constants.ZipMimeType, $"{Constants.AppName}_{DateTime.Now.ToString(Constants.DateTimeFormat)}.{Constants.CompressedArchiveFileType}");
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else //this case can only be a single file in the collection
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
                            else 
                            {
                                throw new CloudFunctionStopException("Invalid data in request"); 
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                AddNewNotification(new Warning($"No file(s) or folder(s) were chosen for download"));
            }
            catch (Exception)
            {
                AddNewNotification(new Error($"App unable to download requested item(s)"));
            }

            return returnAction;
        }

        /// <summary>
        /// Non action method to get a temporary file path for download zip
        /// </summary>
        /// <returns>The path and name for the temporary file</returns>
        [NonAction]
        protected string GetTempFileNameAndPath()
        {
            if (!Directory.Exists(Constants.GetTempFileDirectory()))
            {
                Directory.CreateDirectory(Constants.GetTempFileDirectory());
            }

            return Path.Combine(Constants.GetTempFileDirectory(), Guid.NewGuid().ToString() + DateTime.Now.ToString(Constants.DateTimeFormat));
        }
    }
}
