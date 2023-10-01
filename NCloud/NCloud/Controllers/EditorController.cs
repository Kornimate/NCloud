using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NCloud.Controllers
{
    public class EditorController : Controller
    {
        // GET: EditorController
        public ActionResult Index()
        {
            return View();
        }

        // GET: EditorController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EditorController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EditorController/Create
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

        // GET: EditorController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EditorController/Edit/5
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

        // GET: EditorController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EditorController/Delete/5
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
