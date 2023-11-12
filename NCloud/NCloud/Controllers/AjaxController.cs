using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Services;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    public class AjaxController : CloudControllerDefault
    {
        public AjaxController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier) : base(service, userManager, signInManager, env, notifier) { }

        public IActionResult DeleteItem()
        {
            return ViewComponent("Table",null);//TODO: viewmodel instancing
        }

        public IActionResult ShareItem()
        {
            return ViewComponent("Table",null); //TODO: viewmodel instancing
        }
    }
}
