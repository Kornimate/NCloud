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
        private readonly string DEFAULTEDITOR = nameof(EditorController.TextEditor);

        private readonly Dictionary<string, List<string>> codingExtensionsToActionName = new()
        {
            { "CodeEditor", new List<string>() {".cpp",".cs"} },
            { "TextEditor", new List<string>() {".txt",".doc"} }
        };

        public EditorController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        // GET: EditorController
        public async Task<IActionResult> Index(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);

            string actionName = codingExtensionsToActionName.FirstOrDefault(x => x.Value.Contains(fi.Extension)).Key ?? DEFAULTEDITOR;

            return await Task.FromResult<IActionResult>(RedirectToAction(actionName, new { fileName = fileName }));
        }

        public async Task<IActionResult> CodeEditor(string? fileName = null)
        {
            if (fileName == null)
            {
                return View(new CodeEditorViewModel());
            }
            CloudPathData pathData = await GetSessionUserPathData();
            return View(new CodeEditorViewModel { FilePath = service.ReturnServerPath(Path.Combine(pathData.CurrentPath, fileName)) });
        }

        public async Task<IActionResult> TextEditor(string? fileName = null)
        {
            //TODO: implement with file input
            return await Task.FromResult<IActionResult>(View(new TextEditorViewModel()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEditorData(string? fileNameAndPath = null)
        {
            //TODO: implement save file or create new file
            return await Task.FromResult<IActionResult>(Ok());
        }
    }
}
