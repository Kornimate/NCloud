﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using NCloud.Users;
using NCloud.ViewModels;
using NCloud.Controllers;
using System.IO;
using DNTCaptcha.Core;

namespace ELTE.TodoList.Web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<CloudUser> userManager;
        private readonly SignInManager<CloudUser> signInManager;
        private readonly IWebHostEnvironment env;

        public AccountController(UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.env = env;
        }
        public IActionResult Back(string returnUrl)
        {
            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Index(string returnUrl)
        {
            CloudUser user = await userManager.GetUserAsync(User);
            ViewBag.ReturnUrl = returnUrl;
            return View(new AccountViewModel(user.UserName, user.FullName, user.Email));
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
                    if (returnUrl is null)
                    {
                        return RedirectToAction("Index", "Drive");
                    }
                    return RedirectToLocal(returnUrl);
                }

                ModelState.AddModelError("", "Failed to Login!");
            }

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var existingUserName = await userManager.FindByNameAsync(vm.UserName);
                if (existingUserName != null)
                {
                    ModelState.AddModelError("UserName", "This Username is already in use!");
                    return View(vm);
                }
                var existingEmail = await userManager.FindByNameAsync(vm.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "This Username is already in use!");
                    return View(vm);
                }
                var user = new CloudUser { UserName = vm.UserName, FullName = vm.FullName,Email=vm.Email };
                var result = await userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    CloudUser newUser = await userManager.FindByNameAsync(vm.UserName);
                    CreateBaseDirectory(newUser);
                    await signInManager.SignInAsync(newUser,false);
                    return RedirectToAction(nameof(Index), "Drive");
                }

                ModelState.AddModelError("", "Failed to Register!");
            }

            return View(vm);
        }

        [NonAction]
        private void CreateBaseDirectory(CloudUser cloudUser)
        {
            string publicFolder = Path.Combine(env.WebRootPath, "CloudData", "Public");
            if (!Directory.Exists(publicFolder))
            {
                Directory.CreateDirectory(publicFolder);
            }
            string userFolderPath = Path.Combine(env.WebRootPath, "CloudData", "UserData",cloudUser.Id);
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
            }
            Directory.CreateDirectory(Path.Combine(userFolderPath,"Documents"));
            Directory.CreateDirectory(Path.Combine(userFolderPath,"Pictures"));
            Directory.CreateDirectory(Path.Combine(userFolderPath,"Videos"));
            Directory.CreateDirectory(Path.Combine(userFolderPath,"Music"));
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
