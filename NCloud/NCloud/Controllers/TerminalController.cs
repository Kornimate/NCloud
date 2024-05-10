using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.DTOs;
using NCloud.Models;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NCloud.Controllers
{
    [Authorize]
    public class TerminalController : CloudControllerDefault
    {
        private readonly ICloudTerminalService terminalService;
        public TerminalController(ICloudTerminalService terminalService, ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier)
        {
            this.terminalService = terminalService;
        }

        public async Task<IActionResult> Index(string? currentPath = null)
        {
            currentPath ??= (await GetSessionCloudPathData()).CurrentPathShow;
            return View(new TerminalViewModel
            {
                CurrentDirectory = currentPath,
                ClientSideCommands = terminalService.GetClientSideCommands(),
                ServerSideCommands = terminalService.GetServerSideCommands()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Evaluate([FromBody] string command)
        {
            command = command.Trim();

            if (String.IsNullOrEmpty(command))
                return Json(new ConnectionDTO { Success = true, Message = "", Result = "", Payload = "" });

            try
            {
                CloudTerminalTokenizationManager.CheckCorrectnessOfCommand(command);

                var commandAndParamertes = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                var successAndMsgAndPayLoadAndPrint = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second);

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFile> files)
                    return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item1 ? Constants.TerminalGreenText(successAndMsgAndPayLoadAndPrint.Item2) : Constants.TerminalRedText(successAndMsgAndPayLoadAndPrint.Item2), Payload = String.Empty, Result = $"\n{String.Join('\n', files.Select(x => x.Info.Name))}\n\n" });

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFolder> folders)
                    return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item1 ? Constants.TerminalGreenText(successAndMsgAndPayLoadAndPrint.Item2) : Constants.TerminalRedText(successAndMsgAndPayLoadAndPrint.Item2), Payload = String.Empty, Result = $"\n{String.Join('\n', folders.Select(x => x.Info.Name))}\n\n" });

                return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item1 ? Constants.TerminalGreenText(successAndMsgAndPayLoadAndPrint.Item2) : Constants.TerminalRedText(successAndMsgAndPayLoadAndPrint.Item2), Payload = (!successAndMsgAndPayLoadAndPrint.Item4 ? successAndMsgAndPayLoadAndPrint.Item3?.ToString() : (await GetSessionCloudPathData()).CurrentPathShow) ?? String.Empty, Result = !String.IsNullOrEmpty(successAndMsgAndPayLoadAndPrint.Item3?.ToString() ?? String.Empty) && successAndMsgAndPayLoadAndPrint.Item4 ? ($"\n{successAndMsgAndPayLoadAndPrint.Item3}\n") : "" });

            }
            catch (InvalidDataException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = Constants.TerminalRedText($"invalid command - {ex.Message}") });

            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = Constants.TerminalRedText("error while executing command") });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluateSingleLine(string command)
        {
            if (command is null || command == String.Empty)
            {
                AddNewNotification(new Error("No command"));

                return RedirectToAction("Details", "Drive");
            }

            try
            {

                CloudTerminalTokenizationManager.CheckCorrectnessOfSingleLineCommand(command);

                var commandAndParamertes = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                var successAndMsgAndPayLoadAndPrint = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second);

                CloudNotificationAbstarct notification = successAndMsgAndPayLoadAndPrint.Item1 ? new Success(successAndMsgAndPayLoadAndPrint.Item2) : new Error(successAndMsgAndPayLoadAndPrint.Item2);

                AddNewNotification(notification);

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFile> files)
                    return RedirectToAction("Details", "Drive", new { files = files , folders = new List<CloudFolder>(), passedItems = true});

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFolder> folders)
                    return RedirectToAction("Details", "Drive", new { files = new List<CloudFile>(), folders = folders, passedItems = true });

                return RedirectToAction("Details", "Drive");

            }
            catch (InvalidDataException ex)
            {
                AddNewNotification(new Error($"Invalid command - {ex.Message}"));

                return RedirectToAction("Details", "Drive");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while executing command"));

                return RedirectToAction("Details", "Drive");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckClientSideCommand([FromBody] string command)
        {
            try
            {
                CloudTerminalTokenizationManager.CheckCorrectnessOfCommand(command);

                var commandAndParameters = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                CloudTerminalTokenizationManager.CheckClientSideCommandSyntax(commandAndParameters.First, commandAndParameters.Second.Count, terminalService.GetClientSideCommandsObjectList());

                string elementHTML = await terminalService.GetClientSideCommandHTMLElement(commandAndParameters.First, commandAndParameters.Second);

                return await Task.FromResult<JsonResult>(Json(new CommandDTO { IsClientSide = true, ActionHTMLElement = elementHTML, ActionHTMLElementId= Constants.DownloadHTMLElementId, NoErrorWithSyntax = true, ErrorMessage = "" }));
            }
            catch (InvalidOperationException ex)
            {
                return await Task.FromResult<JsonResult>(Json(new CommandDTO { IsClientSide = false, NoErrorWithSyntax = false, ErrorMessage = Constants.TerminalRedText($"invalid command - {ex.Message}") }));
            }
            catch (Exception)
            {
                return await Task.FromResult<JsonResult>(Json(new CommandDTO { IsClientSide = true, NoErrorWithSyntax = false, ErrorMessage = Constants.TerminalRedText("error while executing command") }));
            }
        }

        public async Task<IActionResult> DownloadFolder(string? folderName)
        {
            if (folderName is null || folderName == String.Empty)
            {
                AddNewNotification(new Error("No directory name specified"));

                return RedirectToAction("Index");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFolderStarterSymbol + folderName
            },
            (await GetSessionCloudPathData()).CurrentPath,
            RedirectToAction("Index", "Terminal"));
        }

        public async Task<IActionResult> DownloadFile(string? fileName)
        {
            if (fileName is null || fileName == String.Empty)
            {
                AddNewNotification(new Error("No file name specified"));

                return RedirectToAction("Index");
            }

            return await Download(new List<string>()
            {
                Constants.SelectedFileStarterSymbol + fileName
            },
            (await GetSessionCloudPathData()).CurrentPath,
            RedirectToAction("Index", "Terminal"));
        }
    }
}
