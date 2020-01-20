using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace FrogFoot.Areas.Home.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private UserManager _userManager;
        private ClientService svc = new ClientService();

        public AccountController()
        {
        }

        public AccountController(UserManager userManager, ApplicationSignInManager signInManager)
        {
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("One");
            userManager.UserTokenProvider = new DataProtectorTokenProvider<User>(provider.Create("EmailConfirmation"));
            UserManager = userManager;

            UserManager.UserValidator = new UserValidator<User>(UserManager)
            {
                AllowOnlyAlphanumericUserNames = false
            };

            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public UserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<UserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Require the user to have a confirmed email before they can log on.
            var user = await UserManager.FindByNameAsync(model.Email);

            if (user != null)
            {
                if (!await UserManager.IsEmailConfirmedAsync(user.Id) && !user.IsDeleted)
                {
                    ViewBag.errorMessage = "You must have a confirmed email to log on.";
                    return View("UserUnconfirmed");
                }
                //if user has been deleted
                if (user.IsDeleted)
                    return View("AccountDeleted");
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLogin(string userId)
        {
            var user = svc.GetUser(userId);

            //check that the supplied credentials match up to the users credentials
            if (user == null)
            {
                ModelState.AddModelError("", "Authentication error. Please contact support for assistance.");
                return View("Login", new LoginViewModel());
            }

            if (user.IsDeleted)
            {
                return View("AccountDeleted");
            }

            //sign user in
            await SignInManager.SignInAsync(user, true, true);

            var precinctCookie = new HttpCookie("PrecinctCode", user.Location.PrecinctCode);
            precinctCookie.Expires.AddDays(365);
            HttpContext.Response.Cookies.Add(precinctCookie);

            return RedirectToAction("Packages", "Portal", new { area = "Client" });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ManageISPContact(string userId)
        {
            var user = svc.GetUser(userId);

            //check that the supplied credentials match up to the users credentials
            if (user == null)
            {
                return new HttpNotFoundResult("No user found by userId");
            }

            //sign user in
            await SignInManager.SignInAsync(user, true, true);

            //use this variable to show the ManageISPContact tab on the profile page
            TempData["ShowISPTab"] = true;

            return RedirectToAction("Profile", "Portal", new { area = "Client"});
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        [AllowAnonymous]
        public ActionResult Register(HomeViewModel model, bool? quickOrder)
        {
            ViewBag.IsQuickOrder = quickOrder;

            var vm = new RegisterViewModel
            {
                Locations = svc.GetLocations().Where(l => l.IsActive).OrderBy(l => l.Name).ToList(),
                Estates = svc.GetEstates(),
                ISPs = svc.GetISPs().OrderBy(i => i.Name).ToList()
            };

            return View(vm);
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model, bool? quickOrder)
        {
            //check recaptcha here
            //if (!ModelState.IsValid) return RedirectToAction("Register", new {model, quickOrder});

            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Landline = model.Landline,
                    LocationId = model.LocationId != -1 ? model.LocationId : null,
                    EstateId = model.EstateId,
                    Address = model.Address,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    CreatedDate = DateTime.Now,
                    EmailConfirmed = true,
                    HasAltContact = false
                };

                if (model.HasAltContact)
                {
                    user.HasAltContact = model.HasAltContact;
                    user.AltFirstName = model.AltFirstName;
                    user.AltLastName = model.AltLastName;
                    user.AltEmail = model.AltEmail;
                    user.AltCellNo = model.AltCellNo;
                    user.AltLandline = model.AltLandline;
                }

                User existingUser = UserManager.FindByEmail(model.Email);

                if (existingUser == null)
                {
                    //process the user's ISP contact options
                    user.ClientISPContacts = CreateISPContactList(model.SelectedISPIds, user.Id, quickOrder);
                    user.ClientContactMethods = CreateClientContactMethods(user.Id);

                    IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                    IdentityResult response = await UserManager.AddToRoleAsync(user.Id, UserType.Client.ToString());

                    if (response.Succeeded)
                    {
                        //taken out because email validation has been scrapped
                        //string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                        //resolve users Zone
                        ZoneSync.ProcessUser(user.Id);

                        //if its a user doing quick order the log them in and redirect to Packages page
                        if (quickOrder == true)
                        {
                            await SignInManager.SignInAsync(user, true, true);

                            //check if the user has any zones. If precinct has no zones then redirect user to home page
                            var userLocation = svc.GetLocations().FirstOrDefault(l => l.LocationId == user.LocationId);
                            var precinctHasZones = userLocation != null && svc.CheckPrecinctHasZones(userLocation.PrecinctCode);
                            if (!precinctHasZones && !svc.CheckUserCanOrder(user.Id))
                            {
                                TempData["BlockUserOrder"] = true;
                                return RedirectToAction("Index", "Portal", new { area = "Client", });
                            }

                            return RedirectToAction("Packages", "Portal", new { area = "Client", isQuickOrder = quickOrder });
                        }

                        var email = CreateUserRegisterEmail(user, model.LocationId, model.SuburbName, model.EstateName);
                        EmailSender.SendEmail(email, user);

                    }

                    TempData["RegUserFirstName"] = user.FirstName;
                    return RedirectToAction("Register", "Account");
                }
                else
                {
                    if (existingUser.IsDeleted)
                    {
                        //enable user again
                        svc.EnableUser(user);

                        ZoneSync.ProcessUser(user.Id);

                        //send the user new log in credentials
                        string code = await UserManager.GeneratePasswordResetTokenAsync(existingUser.Id);
                        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        await UserManager.SendEmailAsync(user.Id, "Frogfoot Fibre account activation",
                                "Dear " + user.FirstName + ", " +
                                "<br/><br/>We have reactivated your account. Please choose your password by clicking <a href=\"" + callbackUrl + "\">here</a>" +
                                "<br/><br/> Warm regards," +
                                "<br/><br/> The Frogfoot Fibre team");

                        TempData["UserExistsMessage"] =
                            "An account with your email exists but has been deleted. " +
                            "<br/><br/>We have enabled your account and sent you a confirmation email for you to log on";
                    }
                    else
                    {
                        TempData["UserExistsMessage"] = "An account with your email address has already been registered.";
                    }
                    return RedirectToAction("Register", "Account");
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public async Task<ActionResult> AdminRegister()
        {
            var userModel = TempData["userModel"] as AdminRegisterViewModel;

            if (userModel != null)
            {
                var user = new User
                {
                    UserName = userModel.Email,
                    Email = userModel.Email,
                    EmailConfirmed = true,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    PhoneNumber = userModel.PhoneNumber,
                    LocationId = userModel.LocationId,
                    EstateId = userModel.EstateId,
                    Address = userModel.Address,
                    CreatedDate = DateTime.Now,
                    ISPId = userModel.ISPId,
                    IsChamp = userModel.IsChamp,
                    IsUserTypeAdmin = userModel.IsUserTypeAdmin,
                    UsePrecinctCodeForChamp = userModel.UsePrecinctCodeForChamp
                };

                var existingUser = await UserManager.FindByEmailAsync(userModel.Email);

                if (existingUser == null)
                {
                    IdentityResult result = await UserManager.CreateAsync(user, userModel.Password);
                    IdentityResult response = await UserManager.AddToRoleAsync(user.Id, userModel.Type.ToString());
                    if (response.Succeeded)
                    {
                        //Send an email with this link
                        if (userModel.Type != UserType.FFUser && userModel.Type != UserType.Admin)
                        {
                            //Email confirmation taken out

                            //string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                            //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

                            var email = new EmailDto
                            {
                                Subject = "Confirm your account",
                                Body =
                                    "Welcome to the Frogfoot Fibre portal." +
                                    "<br/><br/>Please use the following details to log on." +
                                    "<br/><br/><strong>Username: " + user.Email +
                                    "<br/><br/><strong>Password: " + userModel.Password +
                                    "<br/><br/> Warm regards," +
                                    "<br/><br/> The Frogfoot Fibre team"
                            };

                            EmailSender.SendEmail(email, user);
                        }
                        return RedirectToAction("Index", "ManageUser", new { area = "Admin" });
                    }
                }
                else
                {
                    if (existingUser.IsDeleted)
                    {
                        //Email confirmation taken out

                        //string code = await UserManager.GenerateEmailConfirmationTokenAsync(existingUser.Id);
                        //var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = existingUser.Id, code = code },protocol: Request.Url.Scheme);

                        svc.EnableUser(existingUser);

                        var email = new EmailDto
                        {
                            Subject = "Confirm your account",
                            Body =
                                "Welcome back to the Frogfoot Fibre portal." +
                                "<br/><br/>Please use the following details to log on." +
                                "<br/><br/><strong>Username: " + existingUser.Email +
                                "<br/><br/><strong>Password: " + userModel.Password +
                                "<br/><br/> Warm regards," +
                                "<br/><br/> The Frogfoot Fibre team"
                        };

                        EmailSender.SendEmail(email, user);
                    }
                    else
                    {
                        TempData["UserExists"] = true;
                        return RedirectToAction("AddUser", "ManageUser", new { area = "Admin" });
                    }
                }
            }
            return RedirectToAction("Index", "ManageUser", new { area = "Admin" });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new User { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            if (Request.Cookies["ISPAdmin"] != null)
            {
                var ispAdminCookie = new HttpCookie("ISPAdmin") { Expires = DateTime.Now.AddDays(-1) };
                Response.Cookies.Add(ispAdminCookie);
            }

            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Private Methods 
        private EmailDto CreateUserRegisterEmail(User user, int? locationId, string suburbName, string estateName)
        {
            var email = new EmailDto();

            //if the user is in "Other" suburb (none)
            if (locationId == -1)
            {
                email.Subject =
                    "Welocome to the Frogfoot Fibre portal";
                email.Body = "Dear " + user.FirstName + "," +
                             "<br/><br/>Thank you for registering interest in Frogfoot Fibre." +

                             "<br/><br/>We do <strong>not</strong> as yet have an active Fibre to the Home (FTTH) project in your suburb. " +

                             "<br/><br/>If your suburb falls within our <a href='https://maps.frogfoot.net/coverage'>fibre network coverage area</a>, AND there is sufficient enthusiastic interest from a " +
                             "significant number of your neighbours, we’d be happy to focus some resources on bringing your suburb into the Frogfoot FTTH programme.  " +
                             "Here’s how you can become an active part of that:" +
                             "<br/><br/><ol><li>Share this opportunity with your neighbours, the neighbourhood Facebook group, your ratepayers association and neighbourhood watch. " +
                             "Get your local town councillor involved.  Get people to Register on the portal and place a pin in the map over their residence.</ li>" +
                             "<li>Contact Frogfoot (ftth@frogfoot.com) and offer to be the “Fibre Champion” for your suburb, if no one has already volunteered. We need a local resident " +
                             "to coordinate the mobilisation of your suburb.  The more residents who sign up, the more likely it is that Frogfoot Fibre will reach your suburb before anywhere else.</ li></ol>" +

                             "<br/><br/>If your suburb is not in our fibre network coverage area, we’re sorry, we’d love to help, but it is quite possible that we won’t be able to.  Check back from time to time though." +

                             "<br/><br/> Warm regards," +
                             "<br/><br/> The Frogfoot Fibre team";
            }
            else
            {
                email.Subject =
                    "Welcome to the Frogfoot Fibre portal";
                email.Body = "Dear " + user.FirstName + "," +
                             "<br/><br/>Thank you for registering interest in Frogfoot Fibre in " + suburbName + " " + estateName + ". " +
                             "<br/><br/>To check on the availability of fibre in your immediate area, please log in to the portal and check on the “zone” status under your profile. We do accept pre-orders before a zone is " +
                             "scheduled should you be prepared to wait. Alternatively you can order when we begin construction, at which point we will notify you via the contact details you have provided." +
                             "<br/><br/> Feel free to check the portal regularly for progress updates." +

                             "<br/><br/> Warm regards," +
                             "<br/><br/> The Frogfoot Fibre team";
            }

            return email;
        }

        private List<ClientISPContact> CreateISPContactList(List<int?> selectedISPids, string userId, bool? isQuickOrder)
        {
            var contactList = new List<ClientISPContact>();

            //create clientContacts for any not null ids in selectedISPids
            if (selectedISPids.Any(i => i != null) && isQuickOrder != true)
            {
                contactList.AddRange(
                    selectedISPids.Where(i => i != null).Select(
                        id => new ClientISPContact { ISPId = (int)id, UserId = userId, IsISPSelected = true }));
            }
            else
            {
                //if selectedISPs only has null vals -> create all ISPs as contacts
                //if isQuickOrder == true -> create all ISPs as contacts, user must then either order or manage contacts
                var isps = svc.GetISPs();
                contactList.AddRange(
                    isps.Select(i => new ClientISPContact { ISPId = (int)i.ISPId, UserId = userId, IsISPSelected = true }));
            }

            return contactList;
        }

        private List<ClientContactMethod> CreateClientContactMethods(string userId)
        {
            var contactMethods = new List<ClientContactMethod>();

            for (int i = 1; i < 5; i++)
            {
                contactMethods.Add(new ClientContactMethod
                {
                    IsSelected = true,
                    UserId = userId, 
                    ContactMethodId = i
                });
            }

            return contactMethods;

            //return contactMethods.Select(contactMethod => new ClientContactMethod
            //{
            //    IsSelected = true,
            //    ContactMethodId = contactMethod.ContactMethodId,
            //    UserId = userId
            //}).ToList();
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}