using Microsoft.AspNetCore.Mvc;

namespace NCloud.Controllers
{
    public class CloudSpaceRequestController : Controller
    {
        public async Task<IActionResult> Create()
        {
            return View();
        }
    }
}
