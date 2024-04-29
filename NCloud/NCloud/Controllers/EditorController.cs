using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Models;
using System.Text.Json;
using NCloud.ViewModels;
using Microsoft.AspNetCore.Identity;
using NCloud.Users;
using NCloud.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NCloud.Controllers
{
    public class EditorController : CloudControllerDefault
    {
        public EditorController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<IActionResult> Index()
        {
            return View(new EditorIndexViewModel
            {
                CodingExtensions = new SelectList(await ExtensionManager.GetCodingExtensions()),
                TextDocumentExtensions = new SelectList(await ExtensionManager.GetTextDocumentExtensions())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewFile([Bind("FileName,Extension")] EditorIndexViewModel vm)
        {
            if(ModelState.IsValid)
            {

            }

            AddNewNotification(new Error("Invalid input data"));

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditorHub(string fileName)
        {
            if (await ExtensionManager.TryGetFileCodingExtensionData(fileName, out string codingExtensionData))
            {
                return RedirectToAction("CodeEditor", new { fileName = fileName, extensionData = codingExtensionData, errorAction = RedirectToAction("Details", "Drive") });
            }
            else if (await ExtensionManager.TryGetFileCodingExtensionData(fileName, out string textDocumentExtensionData))
            {
                return RedirectToAction("TextEditor", new { fileName = fileName, extensionData = textDocumentExtensionData, errorAction = RedirectToAction("Details", "Drive") });
            }
            else
            {
                AddNewNotification(new Error("No Editor available for this extension"));

                return RedirectToAction("Details", "Drive");
            }
        }

        public async Task<IActionResult> CodeEditor(string fileName, string extensionData, IActionResult errorAction)
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            string pathAndName = Path.Combine(pathData.CurrentPath, fileName);

            try
            {
                return View(new EditorViewModel
                {
                    FilePath = pathAndName,
                    Content = System.IO.File.ReadAllText(service.ServerPath(Path.Combine())),
                    ExtensionData = extensionData
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Application could not load the file"));

                return errorAction;
            }
        }

        public async Task<IActionResult> TextEditor(string fileName, string extensionData, IActionResult errorAction)
        {
            CloudPathData pathData = await GetSessionCloudPathData();

            string pathAndName = Path.Combine(pathData.CurrentPath, fileName);

            try
            {
                return View(new EditorViewModel
                {
                    FilePath = pathAndName,
                    Content = System.IO.File.ReadAllText(service.ServerPath(Path.Combine())),
                    ExtensionData = extensionData
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Application could not load the file"));

                return errorAction;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveData(string? file = null)
        {


            return Json(new ConnectionDTO { Success = true, Message = "---" });
        }
    }
}
