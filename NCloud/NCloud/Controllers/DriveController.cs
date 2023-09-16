using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using Newtonsoft.Json;

namespace NCloud.Controllers
{
    public class DriveController : Controller
    {
        private readonly ICloudService service;
        private readonly UserManager<CloudUser> userManager;
        private readonly SignInManager<CloudUser> singInManager;
        private const string FOLDERSEPARATOR = "//";
        private const string COOKIENAME = "pathData";

        public DriveController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager)
        {
            this.service = service;
            this.userManager = userManager;
            this.singInManager = signInManager;
        }

        // GET: DriveController
        public ActionResult Index()
        {
            var result = service.GetCurrentUserIndexData();
            return View(new DriveIndexViewModel(result.Item1, result.Item2));
        }

        // GET: DriveController/Details/5
        public async Task<ActionResult> Details(string? folderName = null)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                singInManager.SignInAsync(service.GetAdmin(), false).Wait();
            }
            CloudUser user = await userManager.GetUserAsync(User);
            PathData pathdata = null!;
            if (HttpContext.Session.Keys.Contains(COOKIENAME))
            {
                if (folderName is not null)
                {
                    pathdata = JsonConvert.DeserializeObject<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
                }
                else
                {
                    return BadRequest();
                }
            }
            else
            {
                pathdata = new PathData(user.Id);
                HttpContext.Session.SetString(COOKIENAME, JsonConvert.SerializeObject(pathdata));
            }
            string currentPath = pathdata.CurrentPath + FOLDERSEPARATOR + folderName;
            pathdata.PreviousDirectories.Add(folderName!);
            pathdata.CurrentPath += $@"{currentPath}"; //for security reasons
            pathdata.CurrentDirectory = folderName!;
            HttpContext.Session.SetString(COOKIENAME, JsonConvert.SerializeObject(pathdata));
            return View(new DriveDetailsViewModel(service.GetCurrentDeptFiles(currentPath),
                                                service.GetCurrentDeptFolders(currentPath),
                                                                              currentPath));
        }

        public IActionResult Back()
        {
            string? folder = null;
            PathData pathdata = JsonConvert.DeserializeObject<PathData>(HttpContext.Session.GetString(COOKIENAME)!)!;
            if (pathdata.PreviousDirectories.Count > 0)
            {
                folder = pathdata.PreviousDirectories.Last();
                folder ??= String.Empty;
                pathdata.PreviousDirectories.RemoveAt(pathdata.PreviousDirectories.Count - 1);
                List<string> folders = pathdata.CurrentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (folders.Count >= 2)
                {
                    folders.RemoveAt(folders.Count - 1);
                    pathdata.CurrentPath = String.Join(@"//", folders);
                    pathdata.CurrentDirectory = folder;
                    HttpContext.Session.SetString(COOKIENAME, JsonConvert.SerializeObject(pathdata));
                }
            }
            return RedirectToAction("Details", new { folderName = folder });
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
    }
}
