using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Models;
using System.Text.Json;
using NCloud.ViewModels;
using Microsoft.AspNetCore.Identity;
using NCloud.Users;
using NCloud.DTOs;

namespace NCloud.Controllers
{
    public class EditorController : CloudControllerDefault
    {
        public EditorController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public async Task<IActionResult> Index()
        {
            return await Task.FromResult<IActionResult>(View());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewFile(string fileName, string fileExtension)
        {
            //TODO: implement method
            return View();
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
