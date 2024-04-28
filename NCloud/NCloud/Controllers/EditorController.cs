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

        public IActionResult Index(string fileName)
        {
            if (ExtensionManager.TryGetFileCodingExtensionData(fileName, out string codingExtensionData))
            {
                return RedirectToAction("CodeEditor", new { fileName = fileName, extensionData = codingExtensionData });
            }
            else if (ExtensionManager.TryGetFileCodingExtensionData(fileName, out string textDocumentExtensionData))
            {
                return RedirectToAction("TextEditor", new { fileName = fileName, extensionData = textDocumentExtensionData });
            }
            else
            {
                AddNewNotification(new Error("No Editor available for this extension"));

                return RedirectToAction("Details", "Drive");
            }
        }

        public async Task<ActionResult> CodeEditor(string fileName, string extensionData)
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

                return RedirectToAction("Details", "Drive");
            }
        }

        public async Task<IActionResult> TextEditor(string fileName, string extensionData)
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

                return RedirectToAction("Details", "Drive");
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
