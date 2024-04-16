using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NCloud.Controllers
{
    [AllowAnonymous]
    public class WebController : Controller
    {
        public async Task<IActionResult> Download (string path)
        {
            return await Task.FromResult<IActionResult>(Content(path));
        }

        public async Task<IActionResult> Details(string path)
        {
            return await Task.FromResult<IActionResult>(Content(path));
        }
    }
}
