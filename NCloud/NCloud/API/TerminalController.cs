using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;

namespace NCloud.API
{
    public class TerminalController : Controller
    {
        public IActionResult Evaluate(string? input)
        {
            return Content("Hello Terminal!");
        }
    }
}
