using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Models;
using System.Text.Json;
using NCloud.ViewModels;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using NCloud.Users;

namespace NCloud.Controllers
{
    public class EditorController : CloudControllerDefault
    {
        public EditorController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, INotyfService notifier) : base(service, userManager, signInManager, env, notifier) { }
        
        // GET: EditorController
        public ActionResult Index(string? fileName = null)
        {
            if (fileName == null)
            {
                return View(new CodeEditorViewModel());
            }
            PathData pathData = GetSessionUserPathData();
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
    }
}
