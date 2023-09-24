using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using System.Drawing.Drawing2D;
using System.Text.Json;
using PathData = NCloud.Models.PathData;

namespace NCloud.Controllers
{
    public class DriveController : Controller
    {
        private readonly ICloudService service;
        private readonly IWebHostEnvironment env;
        private readonly INotyfService notifier;
        private readonly UserManager<CloudUser> userManager;
        private readonly SignInManager<CloudUser> signInManager;
        private const string FOLDERSEPARATOR = "//";
        private const string COOKIENAME = "pathData";
        private const string ROOTNAME = "@CLOUDROOT";

        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier)
        {
            this.service = service;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
            this.notifier = notifier;
        }

        // GET: DriveController
        public ActionResult Index()
        {
            Task.Run(async () => await signInManager.PasswordSignInAsync("Admin", "Admin_1234", false, false)).Wait();
            var result = service.GetCurrentUserIndexData();
            return View(new DriveIndexViewModel(result.Item1, result.Item2));
        }

        // GET: DriveController/Details/5
        public async Task<ActionResult> Details(string? folderName = null)
        {
            PathData pathdata = null!;
            if (folderName is null)
            {
                if (HttpContext.Session.Keys.Contains(COOKIENAME))
                {
                    pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
                }
                else
                {
                    pathdata = new PathData();
                    pathdata.SetDefaultPathData((await userManager.GetUserAsync(User)).Id.ToString());
                }
            }
            else
            {
                if (HttpContext.Session.Keys.Contains(COOKIENAME))
                {
                    pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
                }
                else
                {
                    return BadRequest();
                }
            }
            string currentPath = pathdata.SetFolder(folderName);
            HttpContext.Session.SetString(COOKIENAME, JsonSerializer.Serialize<PathData>(pathdata));
            ViewBag.CurrentPath = pathdata.CurrentPathShow;
            return View(new DriveDetailsViewModel(service.GetCurrentDeptFiles(currentPath),
                                                service.GetCurrentDeptFolders(currentPath),
                                                                              currentPath));
        }

        public IActionResult Back()
        {
            PathData pathdata = JsonSerializer.Deserialize<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
            if (pathdata.PreviousDirectories.Count > 2)
            {
                pathdata.RemoveFolderFromPrevDirs();
                HttpContext.Session.SetString(COOKIENAME, JsonSerializer.Serialize<PathData>(pathdata));
            }
            return RedirectToAction("Details", "Drive");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddNewFolder(string? folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if(!service.CreateDirectory(folderName!, GetSessionPathData().CurrentPath))
                {
                    throw new Exception("Unknown Error occured");
                }
                notifier.Success("Folder is created!");
            }
            catch (Exception ex)
            {
                TempData["FolderError"] = ex.Message;
                notifier.Error("Failed to create Folder!");
            }
            return RedirectToAction("Details");
        }

        public IActionResult DeleteFolder(string folderName)
        {
            try
            {
                if (folderName is null || folderName == String.Empty)
                {
                    throw new Exception("Folder name must be at least one charachter!");
                }
                if (!service.RemoveDirectory(folderName!, GetSessionPathData().CurrentPath))
                {
                    throw new Exception("Folder is System Folder!");
                }
                notifier.Success("Folder is removed!");
            }
            catch (Exception)
            {
                notifier.Error("Failed to remove Folder!");
            }
            return RedirectToAction("Details");
        }

        public IActionResult SharedIndex()
        {
            return Content("Success");
        }

        // GET: DriveController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DriveController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DriveController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: DriveController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: DriveController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: DriveController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [NonAction]
        private PathData GetSessionPathData()
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
    }
}
