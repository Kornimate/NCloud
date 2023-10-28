using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Models;
using System.Text.Json;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    public class EditorController : Controller
    {
        private readonly ICloudService service;
        private const string COOKIENAME = "pathData";

        public EditorController(ICloudService service)
        {
            this.service = service;
        }
        // GET: EditorController
        public ActionResult Index(string? fileName = null)
        {
            if (fileName == null)
            {
                return View(new CodeEditorViewModel());
            }
            PathData pathData = GetSessionPathData();
            return View(new CodeEditorViewModel { FilePath = service.ReturnServerPath(Path.Combine(pathData.CurrentPath, fileName)) });
        }

        public IActionResult IndexText()
        {
            //TODO: implement with file input
            return View(new TextEditorViewModel());
        }

        [HttpPost]
        [ActionName("IndexText")]
        [ValidateAntiForgeryToken]
        public IActionResult IndexTextPost(string? fileNameAndPath = null)
        {
            //TODO: implement save file or create new file
            return View(new TextEditorViewModel());
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
