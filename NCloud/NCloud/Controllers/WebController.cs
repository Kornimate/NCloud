using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCloud.Security;

namespace NCloud.Controllers
{
    [AllowAnonymous]
    public class WebController : Controller
    {
        public async Task<IActionResult> Details(string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(View());
        }

        public async Task<IActionResult> Back(string path)
        {
            return null!;
        }

        public async Task<IActionResult> Download (string path)
        {
            path = HashManager.DecryptString(path);

            return await Task.FromResult<IActionResult>(Content(path));
        }
    }
}
