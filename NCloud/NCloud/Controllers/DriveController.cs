using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult> Details(int id,int? parentId = null, string? currentPath = null, string? folderName = null)
        {
            CloudUser user = await userManager.GetUserAsync(HttpContext.User);
            if (!HttpContext.Session.Keys.Contains("prevFolders"))
            {
                HttpContext.Session.SetString("prevFolders", JsonConvert.SerializeObject(new List<int?> { parentId}));
            }
            else
            {
                List<int?> folders = JsonConvert.DeserializeObject<List<int?>>(HttpContext.Session.GetString("prevFolders")!)!;
                folders.Add(parentId);
                HttpContext.Session.SetString("prevFolders", JsonConvert.SerializeObject(folders));
            }
            if (currentPath is null)
            {
                ViewData["currentPath"] = @"@CLOUDROOT::";
            }
            else
            {
                ViewData["currentPath"] = currentPath + $@"{(folderName is null ? "" : "//" + folderName)}";
            }
            return View(new DriveDetailsViewModel(service.GetCurrentDeptData(id, user), id));
        }

        public IActionResult Back(string currentPath)
        {
            int? parentId = 0;
            List<int?> prevFolders = JsonConvert.DeserializeObject<List<int?>>(HttpContext.Session.GetString("prevFolders")!)!;
            if (prevFolders?.Count > 0)
            {
                parentId = prevFolders.Last();
                parentId ??= 0;
                prevFolders.RemoveAt(prevFolders.Count - 1);
                currentPath = currentPath ?? string.Empty;
                List<string> folders = currentPath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                folders.RemoveAt(folders.Count - 1);
                currentPath = String.Join(@"//", folders);
                ViewData["currentPath"] = currentPath;
                HttpContext.Session.SetString("prevFolders", JsonConvert.SerializeObject(prevFolders));
            }
            return RedirectToAction("Details", new { id = parentId, currentPath = currentPath });
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
