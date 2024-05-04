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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NCloud.ConstantData;
using HarfBuzzSharp;

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

        public async Task<IActionResult> Back(string? redirectionString = null)
        {
            if (redirectionString is null || redirectionString == String.Empty)
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("Index"));
            }

            var redirection = RedirectionManager.CreateRedirectionAction(redirectionString);

            return await Task.FromResult<IActionResult>(RedirectToAction(redirection.Action, redirection.Controller));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewFile([Bind("FileName,Extension")] EditorIndexViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName = String.Join(Constants.FileExtensionDelimiter, vm.FileName, vm.Extension);
                    string path = Constants.GetDefaultFileSavingPath((await userManager.GetUserAsync(User)).Id);

                    string res = await service.CreateFile(new FormFile(new MemoryStream(), 0, 0, vm.FileName, fileName), path, User);

                    if (res != fileName)
                    {
                        AddNewNotification(new Warning($"The file has been renamed!"));
                    }
                    else if (res == String.Empty)
                    {
                        throw new Exception("Error while creating file!");
                    }

                    AddNewNotification(new Success($"File successfully created at {Constants.GetDefaultFileShowingPath()}"));

                    return await EditorHub(fileName, path, RedirectionManager.CreateRedirectionString("Editor", "Index"));
                }
                catch (Exception)
                {
                    AddNewNotification(new Error($"Error while creating file"));

                    return RedirectToAction("Index");
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                AddNewNotification(new Error(error.ErrorMessage));
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> EditorHub(string fileName, string? path = null, string? redirectData = null)
        {
            bool codingExtension = await ExtensionManager.TryGetFileCodingExtensionData(fileName, out string codingExtensionData);
            bool textDocumentExtension = await ExtensionManager.TryGetFileTextDocumentExtensionData(fileName, out string textDocumentExtensionData);

            if (codingExtension && textDocumentExtension)
            {
                return View("Select", new EditorSelectViewModel(fileName, path, codingExtensionData, textDocumentExtensionData, redirectData ?? RedirectionManager.CreateRedirectionString("Drive", "Details")));
            }
            else if (codingExtension)
            {
                return RedirectToAction("CodeEditor", new { fileName = fileName, path = path, extensionData = codingExtensionData, redirectData = redirectData ?? RedirectionManager.CreateRedirectionString("Drive", "Details") });
            }
            else if (textDocumentExtension)
            {
                return RedirectToAction("TextEditor", new { fileName = fileName, path = path, extensionData = textDocumentExtensionData, redirectData = redirectData ?? RedirectionManager.CreateRedirectionString("Drive", "Details") });
            }

            else
            {
                AddNewNotification(new Error("No Editor available for this extension"));

                if (redirectData is null)
                {
                    return RedirectToAction("Details", "Drive");
                }
                else
                {
                    var redirection = RedirectionManager.CreateRedirectionAction(redirectData);

                    return RedirectToAction(redirection.Action, redirection.Controller);
                }
            }
        }

        public async Task<IActionResult> CodeEditor(string fileName, string? path, string extensionData, string redirectData)
        {
            string pathAndName = Path.Combine(path ?? (await GetSessionCloudPathData()).CurrentPath, fileName);

            try
            {
                return View(new EditorViewModel
                {
                    FilePath = pathAndName.Replace(Path.DirectorySeparatorChar, Constants.PathSeparator),
                    Content = System.IO.File.ReadAllText(service.ServerPath(pathAndName)),
                    ExtensionData = extensionData,
                    Redirection = redirectData
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Application could not load the file"));

                var redirection = RedirectionManager.CreateRedirectionAction(redirectData);

                return RedirectToAction(redirection.Action, redirection.Controller);
            }
        }

        public async Task<IActionResult> TextEditor(string fileName, string? path, string extensionData, string redirectData)
        {
            string pathAndName = Path.Combine(path ?? (await GetSessionCloudPathData()).CurrentPath, fileName);

            try
            {
                return View(new EditorViewModel
                {
                    FilePath = pathAndName.Replace(Path.DirectorySeparatorChar, Constants.PathSeparator),
                    Content = System.IO.File.ReadAllText(service.ServerPath(pathAndName)),
                    ExtensionData = extensionData,
                    Redirection = redirectData
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Application could not load the file"));

                var redirection = RedirectionManager.CreateRedirectionAction(redirectData);

                return RedirectToAction(redirection.Action, redirection.Controller);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveData([FromBody] FileDataViewModel vm)
        {
            if (vm.File is null || vm.File == String.Empty)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Invalid file" });
            }

            if (vm.Content is null)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Invalid path" });
            }

            vm.File = vm.File.Replace(Constants.PathSeparator, Path.DirectorySeparatorChar);

            bool success = await service.ModifyFileContent(vm.File, vm.Content.ReplaceLineEndings());

            if (success)
            {
                return Json(new ConnectionDTO { Success = true, Message = "File content successfully saved" });
            }

            return Json(new ConnectionDTO { Success = false, Message = "Error while saving file content" });
        }
    }
}
