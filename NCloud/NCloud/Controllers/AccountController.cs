using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using NCloud.Users;
using NCloud.ViewModels;
using NCloud.Controllers;

namespace ELTE.TodoList.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<CloudUser> userManager;
        private readonly SignInManager<CloudUser> signInManager;

        public AccountController(UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(vm.UserName);
                if (user == null)
                {
                    ModelState.AddModelError("", "No User with this UserName!");
                    return View(vm);
                }

                var result = await signInManager.PasswordSignInAsync(user, vm.Password, false, false);

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError("", "Failed to Login!");
            }

            return View(vm);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
			ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
			ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var existing = await userManager.FindByNameAsync(vm.UserName);
                if (existing != null)
                {
                    ModelState.AddModelError("", "This UserName is already in use!");
                    return View(vm);
                }
                var user = new CloudUser { UserName = vm.UserName, FullName=vm.FullName};
                var result = await userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index),"Account");
                }

                ModelState.AddModelError("", "Failed to Register!");
            }

            return View(vm);
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}
