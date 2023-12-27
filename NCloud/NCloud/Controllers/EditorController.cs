using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Models;
using System.Text.Json;
using NCloud.ViewModels;
using Microsoft.AspNetCore.Identity;
using NCloud.Users;

namespace NCloud.Controllers
{
    public class EditorController : CloudControllerDefault
    {
        private readonly HashSet<string> codingExtensions = new HashSet<string>();

        public EditorController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }
        
        // GET: EditorController
        public IActionResult Index(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            if (codingExtensions.Contains(fi.Extension))
            {
                return RedirectToAction("CodeEditor", new { fileName = fileName });
            }
            else
            {
                return RedirectToAction("TextEditor", new { fileName = fileName });
            }
        }

        public ActionResult CodeEditor(string? fileName = null)
        {
            if (fileName == null)
            {
                return View(new CodeEditorViewModel());
            }
            PathData pathData = GetSessionUserPathData();
            return View(new CodeEditorViewModel { FilePath = service.ReturnServerPath(Path.Combine(pathData.CurrentPath, fileName)) });
        }

        public IActionResult TextEditor(string? fileName = null)
        {
            //TODO: implement with file input
            return View(new TextEditorViewModel());
        }

        [HttpPost]
        [ActionName("TextEditor")]
        [ValidateAntiForgeryToken]
        public IActionResult TextEditorPost(string? fileNameAndPath = null)
        {
            //TODO: implement save file or create new file
            return View(new TextEditorViewModel());
        }
    }
}
