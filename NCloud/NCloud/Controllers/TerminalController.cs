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

                TerminalTokenizationManager.CheckCorrectnessOfCommand(command);

                var commandAndParamertes = TerminalTokenizationManager.Tokenize(command);

                var successAndMsg = await terminalService.Execute(commandAndParamertes.First,commandAndParamertes.Second);

                return Json(new ConnectionDTO { Success = successAndMsg.First, Message = successAndMsg.Second });

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
        private async Task<IActionResult> EvaluateSingleLine(string command)
        {
            if (command is null || command == String.Empty)
            {
                AddNewNotification(new Error("No command"));

                return RedirectToAction("Details", "Drive");
            }

            try
            {

                TerminalTokenizationManager.CheckCorrectnessOfSingleLineCommand(command);

                var commandAndParamertes = TerminalTokenizationManager.Tokenize(command);

                var successAndMsg = await terminalService.Execute(commandAndParamertes.First, commandAndParamertes.Second);

                return Json(new ConnectionDTO { Success = successAndMsg.First, Message = successAndMsg.Second });

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
