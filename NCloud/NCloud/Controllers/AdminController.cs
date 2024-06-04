using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle actions only possible for admin users
    /// </summary>
    public class AdminController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return await Task.FromResult<IActionResult>(View());
        }
    }
}
