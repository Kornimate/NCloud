using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                CurrentDirectory = currentPath
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Evaluate([FromBody] string command)
        {
            if (command is null || command == String.Empty)
                return Json(new ConnectionDTO { Success = false, Message = "No command" });

            try
            {
                CloudTerminalTokenizationManager.CheckCorrectnessOfCommand(command);

                var commandAndParamertes = CloudTerminalTokenizationManager.Tokenize(command, (await userManager.GetUserAsync(User)).Id.ToString());

                var successAndMsgAndPayLoadAndPrint = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second);

                return Json(new ConnectionDTO { Success = successAndMsgAndPayLoadAndPrint.Item1, Message = successAndMsgAndPayLoadAndPrint.Item2, Payload = !successAndMsgAndPayLoadAndPrint.Item4 ? successAndMsgAndPayLoadAndPrint.Item3 : "", Result = successAndMsgAndPayLoadAndPrint.Item4 ? successAndMsgAndPayLoadAndPrint.Item3 : "", });

            }
            catch (InvalidDataException ex)
            {
                return Json(new ConnectionDTO { Success = false, Message = $"Invalid command - {ex.Message}" });

            }
            catch (Exception)
            {
                return Json(new ConnectionDTO { Success = false, Message = "Invalid command" });
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

                var successAndMsg = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second);

                CloudNotificationAbstarct notification = successAndMsg.Item1 ? new Success(successAndMsg.Item2) : new Error(successAndMsg.Item2);

                AddNewNotification(notification);

                return RedirectToAction("Details", "Drive");

            }
            catch (InvalidDataException ex)
            {
                AddNewNotification(new Error($"Invalid command - {ex.Message}"));

                return RedirectToAction("Details", "Drive");
            }
            catch (Exception)
            {
                AddNewNotification(new Error("error while executing command"));

                return RedirectToAction("Details", "Drive");
            }
        }
    }
}
