using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.ConstantData;
using NCloud.DTOs;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to a handle cloud terminal requests
    /// </summary>
    [Authorize]
    public class TerminalController : CloudControllerDefault
    {
        private readonly ICloudTerminalService terminalService;
        public TerminalController(ICloudTerminalService terminalService, ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger)
        {
            this.terminalService = terminalService;
        }

        /// <summary>
        /// Action method to show terminal
        /// </summary>
        /// <param name="cloudPath">Path to current state</param>
        /// <returns>View with current showable path in it</returns>
        public async Task<IActionResult> Index(string? cloudPath = null)
        {
            try
            {
                cloudPath ??= (await GetSessionCloudPathData()).CurrentPathShow;

                return View(new TerminalViewModel
                {
                    CurrentDirectory = cloudPath,
                    ClientSideCommands = terminalService.GetClientSideCommands(),
                    ServerSideCommands = terminalService.GetServerSideCommands()
                });
            }
            catch (Exception)
            {
                AddNewNotification(new Error("Error while opening cloud terminal"));

                return RedirectToAction("Index", "DashBoard");
            }
        }

        /// <summary>
        /// Action method to evalute command posted from terminal via js
        /// </summary>
        /// <param name="command">command as a string</param>
        /// <returns>Json with result of command in it</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Evaluate([FromBody] string command)
        {
            command = command.Trim();

            if (String.IsNullOrWhiteSpace(command))
                return Json(new ConnectionDTO { Success = true, Message = "", Result = "", Payload = "" });

            try
            {
                CloudTerminalTokenizationManager.CheckCorrectnessOfCommand(command);

                var commandAndParamertes = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                CloudPathData pathData = await GetSessionCloudPathData();

                var successAndMsgAndPayLoadAndPrint = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second, pathData, await userManager.GetUserAsync(User));

                await SetSessionCloudPathData(pathData);

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFile> files)
                    return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item1 ? Constants.TerminalGreenText(successAndMsgAndPayLoadAndPrint.Item2) : Constants.TerminalRedText(successAndMsgAndPayLoadAndPrint.Item2), Payload = String.Empty, Result = $"\n{String.Join('\n', files.Select(x => x.Info.Name))}\n\n" });

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFolder> folders)
                    return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item1 ? Constants.TerminalGreenText(successAndMsgAndPayLoadAndPrint.Item2) : Constants.TerminalRedText(successAndMsgAndPayLoadAndPrint.Item2), Payload = String.Empty, Result = $"\n{String.Join('\n', folders.Select(x => x.Info.Name))}\n\n" });

                return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item1 ? Constants.TerminalGreenText(successAndMsgAndPayLoadAndPrint.Item2) : Constants.TerminalRedText(successAndMsgAndPayLoadAndPrint.Item2), Payload = (!successAndMsgAndPayLoadAndPrint.Item4 ? successAndMsgAndPayLoadAndPrint.Item3?.ToString() : (await GetSessionCloudPathData()).CurrentPathShow) ?? String.Empty, Result = !String.IsNullOrEmpty(successAndMsgAndPayLoadAndPrint.Item3?.ToString() ?? String.Empty) && successAndMsgAndPayLoadAndPrint.Item4 ? ($"\n{successAndMsgAndPayLoadAndPrint.Item3}\n") : "" });

            }
            catch (CloudFunctionStopException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = Constants.TerminalRedText($"invalid command - {ex.Message}") });

            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = Constants.TerminalRedText("error while executing command") });
            }
        }

        /// <summary>
        /// Action method to evalute command posted from drive search bar
        /// </summary>
        /// <param name="command">command as a string</param>
        /// <returns>Json with result of command in it</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EvaluateSingleLine(string command)
        {
            command = command.Trim();

            if (String.IsNullOrWhiteSpace(command))
            {
                AddNewNotification(new Error("No command"));

                return RedirectToAction("Details", "Drive");
            }

            try
            {

                CloudTerminalTokenizationManager.CheckCorrectnessOfSingleLineCommand(command);

                var commandAndParamertes = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                CloudPathData pathData = await GetSessionCloudPathData();

                var successAndMsgAndPayLoadAndPrint = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second, pathData, await userManager.GetUserAsync(User));

                await SetSessionCloudPathData(pathData);

                CloudNotificationAbstarct notification = successAndMsgAndPayLoadAndPrint.Item1 ? new Success(successAndMsgAndPayLoadAndPrint.Item2) : new Error(successAndMsgAndPayLoadAndPrint.Item2);
                
                AddNewNotification(notification);

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFile> files)
                    return RedirectToAction("Details", "Drive", new { files = files , folders = new List<CloudFolder>(), passedItems = true});

                if (successAndMsgAndPayLoadAndPrint.Item3 is List<CloudFolder> folders)
                    return RedirectToAction("Details", "Drive", new { files = new List<CloudFile>(), folders = folders, passedItems = true });

                return RedirectToAction("Details", "Drive");

            }
            catch (CloudFunctionStopException ex)
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

        /// <summary>
        /// Action method to check client side executed commands
        /// </summary>
        /// <param name="command">Command as a string</param>
        /// <returns>Json containing success of action and url to be executed on client side</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CheckClientSideCommand([FromBody] string command)
        {
            try
            {
                CloudTerminalTokenizationManager.CheckCorrectnessOfCommand(command);

                var commandAndParameters = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                CloudTerminalTokenizationManager.CheckClientSideCommandSyntax(commandAndParameters.First, commandAndParameters.Second.Count, terminalService.GetClientSideCommandsObjectList());

                UrlGenerationResult urlDetails = await terminalService.GetClientSideCommandUrlDetails(commandAndParameters.First, commandAndParameters.Second, await GetSessionCloudPathData());

                string elementHTML = GenerateHTMLElementWithUrl(urlDetails);

                return await Task.FromResult<JsonResult>(Json(new CommandDTO { IsClientSide = true, ActionHTMLElement = elementHTML, ActionHTMLElementId= Constants.DownloadHTMLElementId, NoErrorWithSyntax = true, ErrorMessage = "" }));
            }
            catch (CloudFunctionStopException ex)
            {
                return await Task.FromResult<JsonResult>(Json(new CommandDTO { IsClientSide = false, NoErrorWithSyntax = true, ErrorMessage = Constants.TerminalRedText($"invalid command - {ex.Message}") }));
            }
            catch (Exception)
            {
                return await Task.FromResult<JsonResult>(Json(new CommandDTO { IsClientSide = true, NoErrorWithSyntax = false, ErrorMessage = Constants.TerminalRedText("error while executing command") }));
            }
        }

        /// <summary>
        /// Action method to download web shared folder
        /// </summary>
        /// <param name="folderName">Name of folder</param>
        /// <returns>Redirect to download</returns>
        public async Task<IActionResult> DownloadFolder(string? folderName)
        {
            if (String.IsNullOrWhiteSpace(folderName))
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

        /// <summary>
        /// Action method to download web shared file
        /// </summary>
        /// <param name="fileName">Name of file</param>
        /// <returns>Redirect to download</returns>
        public async Task<IActionResult> DownloadFile(string? fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName))
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

        /// <summary>
        /// Private method to generate HTML element (a tag) with url generated from specified details
        /// </summary>
        /// <param name="urlDetails">Url details generated by service</param>
        /// <returns>generated HTML element with url in it</returns>
        /// <exception cref="CloudFunctionStopException">Throw if invalid data is generated from inputs</exception>
        [NonAction]
        private string GenerateHTMLElementWithUrl(UrlGenerationResult urlDetails)
        {
            if (urlDetails is null)
                throw new CloudFunctionStopException("unable to generate URL");

            string url = Url.Action(urlDetails.Action, urlDetails.Controller, urlDetails.Parameters) ?? throw new CloudFunctionStopException("error while generating URL");

            return $"<div style=\"display:none\"><a href=\"{url}\" {(urlDetails.Downloadable ? "download" : "")} id=\"{Constants.DownloadHTMLElementId}\"></a></div>";
        }
    }
}
