using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    public class DriveController : Controller
    {
        private readonly ICloudService service;

        public DriveController(ICloudService service)
        {
            this.service = service;
        }

        // GET: DriveController
        public ActionResult Index()
        {
            var result = service.GetCurrentUserIndexData();
            return View(new DriveIndexViewModel(result.item1,result.item2));
        }

        // GET: DriveController/Details/5
        public ActionResult Details(int id)
        {
            return View();
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
