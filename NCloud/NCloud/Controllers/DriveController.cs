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

        public DriveController(ICloudService service, UserManager<CloudUser> userManager)
        {
            this.service = service;
            this.userManager = userManager;
        }

        // GET: DriveController
        public ActionResult Index()
        {
            var result = service.GetCurrentUserIndexData();
            return View(new DriveIndexViewModel(result.Item1, result.Item2));
        }

        // GET: DriveController/Details/5
        public async Task<ActionResult> Details(int id, int? parentId = null, string? folderName = null)
        {
            CloudUser user = await userManager.GetUserAsync(HttpContext.User);
            if (id == 0 && HttpContext.Session.Keys.Contains("pathData") && parentId is null)
            {
                id = JsonConvert.DeserializeObject<PathData>(HttpContext.Session.GetString("pathData")!)!.CurrentDirectory;
            }
            parentId ??= 0;
            PathData pathdata;
            if (!HttpContext.Session.Keys.Contains("pathData"))
            {
                pathdata = new PathData((int)parentId);
                HttpContext.Session.SetString("pathData", JsonConvert.SerializeObject(pathdata));
            }
            else
            {
                pathdata = JsonConvert.DeserializeObject<PathData>(HttpContext.Session.GetString("pathData")!)!;
                pathdata.PreviousDirectories.Add((int)parentId);
                pathdata.CurrentPath += $@"{(folderName is null ? "" : "//" + folderName)}";
                pathdata.CurrentDirectory = id;
                HttpContext.Session.SetString("pathData", JsonConvert.SerializeObject(pathdata));
            }
            ViewData["currentPath"] = pathdata.CurrentPath;
            return View(new DriveDetailsViewModel(service.GetCurrentDeptData(id, user), id));
        }

        public IActionResult Back()
        {
            int? parentId = 0;
            PathData pathdata = JsonConvert.DeserializeObject<PathData>(HttpContext.Session.GetString("pathData")!)!;
            if (pathdata.PreviousDirectories.Count > 0)
            {
                parentId = pathdata.PreviousDirectories.Last();
                parentId ??= 0;
                pathdata.PreviousDirectories.RemoveAt(pathdata.PreviousDirectories.Count - 1);
                List<string> folders = pathdata.CurrentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                if(folders.Count >= 2)
                {
                    folders.RemoveAt(folders.Count - 1);
                    pathdata.CurrentPath = String.Join(@"//", folders);
                    ViewData["currentPath"] = pathdata.CurrentPath;
                    pathdata.CurrentDirectory = (int)parentId;
                    HttpContext.Session.SetString("pathData", JsonConvert.SerializeObject(pathdata));
                }
            }
            return RedirectToAction("Details", new { id = parentId });
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
