using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NCloud.ConstantData;
using NCloud.DTOs;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;
using System.Text;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle file modification requests
    /// </summary>
    [Authorize]
    public class EditorController : CloudControllerDefault
    {
        public EditorController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }

        /// <summary>
        /// Action method to show editor selector page
        /// </summary>
        /// <returns>View with options for user to choose editor</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                return View(new EditorIndexViewModel
                {
                    CodingExtensions = new SelectList(await CloudExtensionManager.GetCodingExtensions()),
                    TextDocumentExtensions = new SelectList(await CloudExtensionManager.GetTextDocumentExtensions())
                });
            }
            catch (Exception)
            {
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Action method to return to page where editor is accessed from
        /// </summary>
        /// <param name="redirectionString">Special string to handle redirection</param>
        /// <returns>Redirection to caller page or error page if error is present</returns>
        public async Task<IActionResult> Back(string? redirectionString = null)
        {
            if (String.IsNullOrWhiteSpace(redirectionString))
            {
                return await Task.FromResult<IActionResult>(RedirectToAction("Index"));
            }

            var redirection = CloudRedirectionManager.CreateRedirectionAction(redirectionString);

            if (redirection is not null)
                return await Task.FromResult<IActionResult>(RedirectToAction(redirection.Action, redirection.Controller));

            else
                return RedirectToAction("Error", "Home");
        }

        /// <summary>
        /// Action method to handle post request to create new file at @CLOUDROOT/Documents
        /// </summary>
        /// <param name="vm">CreationDetails wrapped in EditorIndexViewModels</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNewFile([Bind("FileName,Extension,Editor")] EditorIndexViewModel vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string fileName = String.Join(Constants.FileExtensionDelimiter, vm.FileName, vm.Extension);

                    string path = Constants.GetDefaultFileSavingPath((await userManager.GetUserAsync(User)).Id);

                    string res = await service.CreateFile(new FormFile(new MemoryStream(), 0, 0, vm.FileName, fileName), path, await userManager.GetUserAsync(User));

                    if (res != fileName)
                    {
                        AddNewNotification(new Warning($"The file has been renamed!"));
                    }

                    AddNewNotification(new Success($"File successfully created at {Constants.GetDefaultFileShowingPath()}"));

                    return await EditorHub(res, path, CloudRedirectionManager.CreateRedirectionString("Editor", "Index"), vm.Editor);
                }
                catch (CloudFunctionStopException ex)
                {
                    AddNewNotification(new Error($"Error - {ex.Message}"));

                    return RedirectToAction("Index");
                }
                catch (CloudLoggerException ex)
                {
                    logger.LogError(ex.Message);

                    AddNewNotification(new Error($"Error while creating file"));

                    return RedirectToAction("Index");
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

        /// <summary>
        /// Action method to select editor for file extension
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="path">Path in app</param>
        /// <param name="redirectData">Special string for redirection</param>
        /// <param name="editorName">String representing the editor to be used</param>
        /// <returns>Selected action method redirection or to selection page (more than one editor can edit the extension)</returns>
        public async Task<IActionResult> EditorHub(string fileName, string? path = null, string? redirectData = null, string? editorName = null)
        {
            bool codingExtension = await CloudExtensionManager.TryGetFileCodingExtensionData(fileName, out string codingExtensionData);
            bool textDocumentExtension = await CloudExtensionManager.TryGetFileTextDocumentExtensionData(fileName, out string textDocumentExtensionData);

            if (editorName is not null)
            {
                if (editorName == Constants.CodeEditor)
                {
                    return RedirectToAction("CodeEditor", new { fileName = fileName, path = path, extensionData = codingExtensionData, redirectData = redirectData ?? CloudRedirectionManager.CreateRedirectionString("Drive", "Details") });
                }
                else if (editorName == Constants.TextEditor)
                {
                    return RedirectToAction("TextEditor", new { fileName = fileName, path = path, extensionData = textDocumentExtensionData, redirectData = redirectData ?? CloudRedirectionManager.CreateRedirectionString("Drive", "Details") });
                }
            }

            if (codingExtension && textDocumentExtension)
            {
                return View("Select", new EditorSelectViewModel(fileName, path, codingExtensionData, textDocumentExtensionData, redirectData ?? CloudRedirectionManager.CreateRedirectionString("Drive", "Details")));
            }
            else if (codingExtension)
            {
                return RedirectToAction("CodeEditor", new { fileName = fileName, path = path, extensionData = codingExtensionData, redirectData = redirectData ?? CloudRedirectionManager.CreateRedirectionString("Drive", "Details") });
            }
            else if (textDocumentExtension)
            {
                return RedirectToAction("TextEditor", new { fileName = fileName, path = path, extensionData = textDocumentExtensionData, redirectData = redirectData ?? CloudRedirectionManager.CreateRedirectionString("Drive", "Details") });
            }

            else
            {
                AddNewNotification(new Error("No editor available for this extension"));

                if (redirectData is null)
                {
                    return RedirectToAction("Details", "Drive");
                }
                else
                {
                    var redirection = CloudRedirectionManager.CreateRedirectionAction(redirectData);

                    if (redirection is not null)
                        return RedirectToAction(redirection.Action, redirection.Controller);

                    else
                        return RedirectToAction("Index", "Editor");
                }
            }
        }

        /// <summary>
        /// Action method to open code editor page
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="path">Path to File</param>
        /// <param name="extensionData">Extension info</param>
        /// <param name="redirectData">Special string for redirection</param>
        /// <returns>View to code editor</returns>
        public async Task<IActionResult> CodeEditor(string fileName, string? path, string extensionData, string redirectData)
        {
            string pathAndName = Path.Combine(path ?? (await GetSessionCloudPathData()).CurrentPath, fileName);

            string fileServerPathAndName = service.ServerPath(pathAndName);

            Encoding fileEncoding = CloudEncodingSupport.GetEncoding(fileServerPathAndName);

            try
            {
                return View(new EditorViewModel
                {
                    FilePath = pathAndName.Replace(Path.DirectorySeparatorChar, Constants.PathSeparator),
                    FileExtension = Path.GetExtension(fileName).ToLower(),
                    Content = System.IO.File.ReadAllText(fileServerPathAndName, fileEncoding),
                    ExtensionData = extensionData,
                    Redirection = redirectData,
                    Encoding = fileEncoding.CodePage.ToString(),
                    EncodingName = fileEncoding.EncodingName
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Application could not load the file"));

                var redirection = CloudRedirectionManager.CreateRedirectionAction(redirectData);

                if (redirection is not null)
                    return RedirectToAction(redirection.Action, redirection.Controller);

                else
                    return RedirectToAction("Index", "Editor");
            }
        }

        /// <summary>
        /// Action method to open text editor page
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <param name="path">Path to File</param>
        /// <param name="extensionData">Extension info</param>
        /// <param name="redirectData">Special string for redirection</param>
        /// <returns>View to text editor</returns>
        public async Task<IActionResult> TextEditor(string fileName, string? path, string extensionData, string redirectData)
        {
            string pathAndName = Path.Combine(path ?? (await GetSessionCloudPathData()).CurrentPath, fileName);

            string fileServerPathAndName = service.ServerPath(pathAndName);

            Encoding fileEncoding = CloudEncodingSupport.GetEncoding(fileServerPathAndName);

            try
            {
                FileInfo fi = new FileInfo(fileServerPathAndName);

                if (fi.Length > Constants.MaximumEditableFileLength)
                    throw new CloudFunctionStopException("File is too big to be edited (maximum 20MB support)");

                return View(new EditorViewModel
                {
                    FilePath = pathAndName.Replace(Path.DirectorySeparatorChar, Constants.PathSeparator),
                    Content = System.IO.File.ReadAllText(fileServerPathAndName, fileEncoding),
                    ExtensionData = extensionData,
                    Redirection = redirectData,
                    Encoding = fileEncoding.CodePage.ToString(),
                    EncodingName = fileEncoding.EncodingName
                });
            }
            catch (CloudFunctionStopException ex)
            {
                AddNewNotification(new Error(ex.Message));

                var redirection = CloudRedirectionManager.CreateRedirectionAction(redirectData);

                if (redirection is not null)
                    return RedirectToAction(redirection.Action, redirection.Controller);

                else
                    return RedirectToAction("Index", "Editor");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Application could not load the file"));

                var redirection = CloudRedirectionManager.CreateRedirectionAction(redirectData);

                if (redirection is not null)
                    return RedirectToAction(redirection.Action, redirection.Controller);

                else
                    return RedirectToAction("Index", "Editor");
            }
        }

        /// <summary>
        /// Action method to handle data saving request from JS (each editor)
        /// </summary>
        /// <param name="vm">Data for save wrapped in FileDataViewModel</param>
        /// <returns>Json with boolean value indication success and message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> SaveData([FromBody] FileDataViewModel vm)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(vm.File))
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Invalid file" });
                }

                if (vm.Content is null)
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Invalid path" });
                }

                if (vm.Encoding is null)
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Invalid file encoding " });
                }

                Encoding? encoding = null;

                try
                {
                    encoding = Encoding.GetEncoding(int.Parse(vm.Encoding));
                }
                catch (Exception)
                {
                    return Json(new ConnectionDTO { Success = false, Message = "Error while retrieving encoding" });
                }

                vm.File = vm.File.Replace(Constants.PathSeparator, Path.DirectorySeparatorChar);

                bool success = await service.ModifyFileContent(vm.File, vm.Content.ReplaceLineEndings(), encoding,  (await userManager.GetUserAsync(User))!);

                if (success)
                {
                    return Json(new ConnectionDTO { Success = true, Message = "File content successfully saved" });
                }

                return Json(new ConnectionDTO { Success = false, Message = "Error while saving file content" });
            }
            catch (FileNotFoundException ex)
            {
                AddNewNotification(new Error($"Error - {ex.Message}"));

                return Json(new ConnectionDTO { Success = false, Redirection = Url.Action("Index", "DashBoard")! });
            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Error - {ex.Message}" });
            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Error while saving file content" });
            }
        }
    }
}
