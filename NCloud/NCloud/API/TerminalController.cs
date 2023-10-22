using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;
using NCloud.Services;

namespace NCloud.API
{
    public class TerminalController : Controller
    {
        private readonly ICloudTerminalService service;
        public TerminalController(ICloudTerminalService service)
        {
            this.service = service;
        }
        public IActionResult Evaluate(string? input)
        {
            string result = service.ExecuteCommand(input);
            return Content(result);
        }
    }
}
