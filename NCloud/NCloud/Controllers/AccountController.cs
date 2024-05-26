using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle Account related requests
    /// </summary>
    [Authorize]
    public class AccountController : CloudControllerDefault
    {
        public AccountController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger) : base(service, userManager, signInManager, env, notifier, logger) { }
        public IActionResult Back(string returnUrl)
        {
            return Redirect(returnUrl);
        }

        /// <summary>
        /// Action to user details page
        /// </summary>
        /// <param name="returnUrl">Url to be returned to</param>
        /// <returns>View of the user details</returns>
        public async Task<IActionResult> Index(string returnUrl)
        {
            CloudUser user = await userManager.GetUserAsync(User);

            ViewBag.ReturnUrl = returnUrl;

            return View(new AccountViewModel(user.UserName, user.FullName, user.Email));
        }

        /// <summary>
        /// Action method to handle user login
        /// </summary>
        /// <param name="returnUrl">Url to be returned to</param>
        /// <returns>Login view</returns>
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        /// <summary>
        /// Action method to handle login post request
        /// </summary>
        /// <param name="vm">Login data inside LoginViewModel class</param>
        /// <param name="returnUrl">Url to be returned to</param>
        /// <returns>Redirection to dashboard, otherwise view with error message</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName,Password")] LoginViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(vm.UserName);

                if (user is null)
                {
                    AddNewNotification(new Error("Invalid username"));
                    return View(vm);
                }

                var result = await signInManager.PasswordSignInAsync(user, vm.Password, false, false);

                if (result.Succeeded)
                {
                    if (returnUrl is null)
                    {
                        return RedirectToAction("Index", "DashBoard");
                    }

                    return await RedirectToLocal(returnUrl);
                }

                AddNewNotification(new Error("Failed to login"));
            }

            return View(vm);
        }

        /// <summary>
        /// Action method to show registration page
        /// </summary>
        /// <param name="returnUrl">Url to be returned to</param>
        /// <returns>Registration view</returns>
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        /// <summary>
        /// Action method to handle registration request
        /// </summary>
        /// <param name="vm">Registration data wrapped in RegisterViewModel class</param>
        /// <param name="returnUrl">Url to be returned to</param>
        /// <returns>Redirection to dashboard, otherwise view with errors</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Please enter the security code as a number.")]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            vm.FullName = $@"{vm.FullName}";
            vm.UserName = $@"{vm.UserName}";
            vm.Password = $@"{vm.Password}";
            vm.PasswordRepeat = $@"{vm.PasswordRepeat}";
            vm.Email = $@"{vm.Email}";

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUserName = await userManager.FindByNameAsync(vm.UserName);

                    if (existingUserName is not null)
                    {
                        AddNewNotification(new Error("Username is already in use"));

                        return View(vm);
                    }
                    var existingEmail = await userManager.FindByNameAsync(vm.Email);

                    if (existingEmail is not null)
                    {
                        AddNewNotification(new Error("This Username is already in use!"));

                        return View(vm);

                    }

                    var user = new CloudUser { UserName = vm.UserName, FullName = vm.FullName, Email = vm.Email };

                    var result = await userManager.CreateAsync(user, vm.Password);

                    if (result.Succeeded)
                    {
                        try
                        {
                            CloudUser newUser = await userManager.FindByNameAsync(vm.UserName);

                            await service.CreateBaseDirectoryForUser(newUser);

                            await signInManager.SignInAsync(newUser, false);

                            return RedirectToAction(nameof(Index), "DashBoard");
                        }
                        catch (CloudFunctionStopException ex)
                        {
                            AddNewNotification(new Error($"Error - {ex.Message}"));

                            return View(vm);
                        }
                        catch (CloudLoggerException ex)
                        {
                            logger.LogError(ex.Message);

                            AddNewNotification(new Error("Error while creating user resources"));


                            if (!await service.RemoveUser(user))
                                logger.LogError($"Error while removing user - {user.UserName}");


                            return View(vm);
                        }
                    }

                    AddNewNotification(new Error("Failed to register"));
                }
                catch
                {
                    AddNewNotification(new Error("Unknow error while registering user"));
                }
            }

            return View(vm);
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            AddNewNotification(new Information("Logout complete"));

            return RedirectToAction("Index", "Home");
        }
    }
}
