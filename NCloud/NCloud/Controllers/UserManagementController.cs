using DNTCaptcha.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NCloud.ConstantData;
using NCloud.Models;
using NCloud.Services;
using NCloud.Services.Exceptions;
using NCloud.Users;
using NCloud.ViewModels;
using System.Text.Encodings.Web;
using System.Text;

namespace NCloud.Controllers
{
    /// <summary>
    /// Class to handle Account related requests
    /// </summary>
    [Authorize]
    public class UserManagementController : CloudControllerDefault
    {
        private readonly IUserStore<CloudUser> userStore;
        private readonly IUserEmailStore<CloudUser> emailStore;
        private readonly IEmailSender emailSender;
        private readonly IEmailTemplateService emailTemplateService;
        public UserManagementController(ICloudService service, UserManager<CloudUser> userManager, SignInManager<CloudUser> signInManager, IWebHostEnvironment env, ICloudNotificationService notifier, ILogger<CloudControllerDefault> logger, IEmailSender emailSender, IUserStore<CloudUser> userStore, IConfiguration config) : base(service, userManager, signInManager, env, notifier, logger)
        {
            this.emailSender = emailSender;
            this.emailTemplateService = new EmailTemplateService(emailSender, config);
            this.userStore = userStore;
            this.emailStore = GetEmailStore();
        }
        public IActionResult Back(string returnUrl)
        {
            try
            {
                return Redirect(returnUrl);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "DashBoard");
            }
        }

        /// <summary>
        /// Action to user details page
        /// </summary>
        /// <param name="returnUrl">Url to be returned to</param>
        /// <returns>View of the user details</returns>
        public async Task<IActionResult> UserPage(string returnUrl)
        {
            CloudUser user = (await userManager.GetUserAsync(User))!;

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
        /// <returns>Redirection to dashboard or to return URL, otherwise view with error message</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName,Password")] LoginViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            vm.UserName = $@"{vm.UserName}";
            vm.Password = $@"{vm.Password}";

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(vm.UserName);

                if (user is null)
                {
                    AddNewNotification(new Error("Invalid login credentials"));
                    return View(vm);
                }

                var result = await signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, false);

                if (result.Succeeded)
                {
                    AddNewNotification(new Success("Successful login"));

                    if (returnUrl is null)
                    {
                        return RedirectToAction("Index", "DashBoard");
                    }

                    logger.LogInformation($"{user.UserName} logged in.");

                    return await RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("/Account/LoginWith2fa", new { area = "Identity", ReturnUrl = returnUrl, RememberMe = vm.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    if ((await userManager.GetRolesAsync(user)).Contains(Constants.AdminRole))
                    {
                        AddNewNotification(new Warning("Successful login, admin was locked out"));

                        if (returnUrl is null)
                        {
                            return RedirectToAction("Index", "DashBoard");
                        }

                        return await RedirectToLocal(returnUrl);
                    }
                    logger.LogWarning($"{vm.UserName} account locked out.");

                    await emailTemplateService.SendEmailAsync(new CloudUserLockedOut(emailTemplateService.GetSelfEmailAddress(), $"User is locked out: {user.UserName}."));

                    return RedirectToPage("/Account/Lockout", new { area = "Identity" });
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
                    var existingEmail = await userManager.FindByEmailAsync(vm.Email);

                    if (existingEmail is not null)
                    {
                        AddNewNotification(new Error("Email is already in use"));

                        return View(vm);

                    }

                    var user = CreateUser();

                    await userStore.SetUserNameAsync(user, vm.UserName, CancellationToken.None);
                    await emailStore.SetEmailAsync(user, vm.Email, CancellationToken.None);

                    user.FullName = vm.FullName;

                    var created = await userManager.CreateAsync(user, vm.Password);
                    var addedToRole = await userManager.AddToRoleAsync(user, Constants.UserRole);


                    if (created.Succeeded && addedToRole.Succeeded)
                    {
                        logger.LogInformation($"{user.UserName} created a new account with password.");

                        await emailTemplateService.SendEmailAsync(new CloudUserRegistration(emailTemplateService.GetSelfEmailAddress(), $"{user.UserName} created a new account."));

                        try
                        {
                            CloudUser? newUser = await userManager.FindByNameAsync(vm.UserName);

                            if (newUser is null)
                                throw new CloudFunctionStopException("no user with this username");

                            await service.CreateBaseDirectoryForUser(newUser);

                            var code = await userManager.GenerateEmailConfirmationTokenAsync(newUser);

                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = newUser.Id, code = code, returnUrl = returnUrl },
                                protocol: Request.Scheme)!;

                            await emailSender.SendEmailAsync(newUser.Email!, "Confirm your email",
                                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                            if (userManager.Options.SignIn.RequireConfirmedAccount)
                            {
                                var value = RedirectToPage("/Account/RegisterConfirmation", new { area = "Identity", email = newUser.Email, returnUrl = returnUrl });
                                return value;
                            }
                            else
                            {
                                await signInManager.SignInAsync(user, isPersistent: false);
                                return RedirectToAction("Index", "DashBoard");
                            }
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

                    await userManager.DeleteAsync(user);

                    AddNewNotification(new Error("Failed to register"));
                }
                catch
                {
                    AddNewNotification(new Error("Unknow error while registering user"));
                }
            }

            return View(vm);
        }

        public async Task<IActionResult> DeleteAccount(string? returnUrl = null)
        {
            if (User.IsInRole(Constants.AdminRole))
            {
                AddNewNotification(new Error("Admin user can not be removed"));

                TempData["ReturnUrl"] = returnUrl;

                return RedirectToAction("Index", "Account");
            }

            CloudUser user = (await userManager.GetUserAsync(User))!;

            try
            {
                await service.DeleteDirectoriesForUser(Constants.GetPrivateBaseDirectoryForUser(user.Id.ToString()), user);

                await signInManager.SignOutAsync();

                await userManager.DeleteAsync(user);

                AddNewNotification(new Information("Account deleted"));

                await emailTemplateService.SendEmailAsync(new CloudUserAccountDeletion(emailTemplateService.GetSelfEmailAddress(), $"{user.UserName} deleted the account."));
            }
            catch (Exception)
            {
                logger.LogError($"Failed to remove user and folders for user: {user.UserName} {user.Id.ToString()}");
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();

            AddNewNotification(new Information("Logout complete"));

            return RedirectToAction("Index", "Home");
        }

        private CloudUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<CloudUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(CloudUser)}'. " +
                    $"Ensure that '{nameof(CloudUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<CloudUser> GetEmailStore()
        {
            if (!userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<CloudUser>)userStore;
        }
    }
}