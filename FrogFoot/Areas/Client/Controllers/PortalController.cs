using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.Client.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using HomeViewModel = FrogFoot.Areas.Client.Models.HomeViewModel;

namespace FrogFoot.Areas.Client.Controllers
{
    [Authorize(Roles = "Client")]
    public class PortalController : Controller
    {
        private ClientService svc = new ClientService();

        private ApplicationSignInManager _signInManager;
        private UserManager _userManager;

        public PortalController()
        {
            
        }

        public PortalController(UserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ActionResult Index()
        {
            var model = new HomeViewModel
            {
                Posts = svc.GetPosts(User.Identity.GetUserId(), PostType.Article),
                User = svc.GetUser(User.Identity.GetUserId()),
            };

            return View(model);
        }

        public ActionResult FAQ()
        {
            return View();
        }

        public ActionResult TermsOfUse()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public async Task<ActionResult> Profile(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new ProfileViewModel
            {
                IndexViewModel = new IndexViewModel
                {
                    HasPassword = HasPassword(),
                    PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                    TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                    Logins = await UserManager.GetLoginsAsync(userId),
                    BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
                },
                User = svc.GetUser(userId),
                Locations = svc.GetLocations(),
                Estates = svc.GetEstates(),
                ISPs = svc.GetISPs(),
                ClientISPs = svc.GetClientISPContact(userId),
                ContactMethods = svc.GetContactMethods(),
                ClientContactMethods = svc.GetClientContactMethods(userId)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(ProfileViewModel model)
        {
            svc.UpdateProfile(model.User);
            TempData["SavedMessage"] = "Your profile has been saved successfully";
            return RedirectToAction("Profile");
        }

        public ActionResult UpdateAltContact(string firstName, string lastName, string email, string cellNo, string landline)
        {
            var user = new User
            {
                Id = User.Identity.GetUserId(),
                HasAltContact = true,
                AltFirstName = firstName,
                AltLastName = lastName,
                AltEmail = email,
                AltCellNo = cellNo,
                AltLandline = landline,
            };

            svc.UpdateAltContact(user);
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateOwnerDetails(bool isOwner, string name, string email, string number)
        {
            var user = new User
            {
                Id = User.Identity.GetUserId(),
                IsOwner = isOwner,
                OwnerName = name,
                OwnerEmail = email,
                OwnerPhoneNumber = number
            };

            svc.UpdateOwnerDetails(user);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Packages(bool? isQuickOrder)
        {
            ViewBag.IsQuickOrder = isQuickOrder;
            var userId = User.Identity.GetUserId();
            var model = new ProductViewModel
            {
                Products = svc.GetProducts(userId),
                ISPs = svc.GetISPs(),
                User = svc.GetUser(userId),
                UserCanOrder = svc.CheckUserCanOrder(userId),
                Specials = svc.GetSpecials()
            };


            if (isQuickOrder == true && model.User.Zone != null)
            {
                //if the status is "Undefined" (ie has no dates yet), then check Early-bird special
                if (model.User.Zone.Status == TrenchingStatus.Undefined)
                {
                    if (model.User.Zone.AllowSpecial)
                    {
                        ViewBag.EarlyBirdSpecial = true;
                    }
                    else
                    {
                        ViewBag.NoEarlyBirdSpecial = true;
                    }
                }

                //if the status is "HasDates" then check Head-start special
                if (model.User.Zone.Status == TrenchingStatus.HasDates)
                {
                    if (model.User.Zone.AllowSpecial)
                    {
                        ViewBag.HeadStartSpecial = true;
                    }
                    else
                    {
                        ViewBag.NoHeadStartSpecial = true;
                    }
                }

                //if the status is "WorkInProgress"
                if (model.User.Zone.Status == TrenchingStatus.WorkInProgress)
                {
                    ViewBag.WorkInProgress = true;
                }

                //if status is "Completed"
                if (model.User.Zone.Status == TrenchingStatus.Completed)
                {
                    ViewBag.Completed = true;
                }
            }

            return View(model);
        }

        public ActionResult WhatIsFTTH()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        /// <summary>
        /// capped == null then show all
        /// capped == true then show capped
        /// capped == false then show uncapped
        /// </summary>
        /// <param name="capped"></param>
        /// <returns></returns>
        public JsonResult GetFilteredProducts(int? ispId, bool? capped, bool? isM2MClientContract)
        {
            var userId = User.Identity.GetUserId();
            var user = UserManager.Users.First(u => u.Id == userId);
            var ids = svc.GetGriddedProductIds(user.LocationId, user.EstateId, ispId, capped, isM2MClientContract);
            return Json(new { ids }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Product(int prodId, ContractTerm? contractTerm, bool? isTwoOptions)
        {
            var userId = User.Identity.GetUserId();
            var model = new ProductViewModel
            {
                Product =  svc.GetProduct(prodId),
                User = svc.GetUser(userId),
                Specials = svc.GetSpecials(),
                UserCanOrder = svc.CheckUserCanOrder(userId)
            };

            ViewBag.ContractTerm = contractTerm;
            ViewBag.IsTwoOptions = isTwoOptions;

            return View(model);
        }

        public ActionResult ConfirmOrder(int prodId, ContractTerm? contractTerm)
        {
            ViewBag.ContractTerm = contractTerm;
            var userId = User.Identity.GetUserId();
            var model = new ProductViewModel
            {
                Product = svc.GetProduct(prodId),
                User = svc.GetUser(userId),
                Orders = svc.GetClientOrders(userId),
                Specials = svc.GetSpecials(),
                ISPs = svc.GetISPs()
            };
            return View(model);
        }

        public ActionResult PlaceOrder(int prodId, int ispId, ContractTerm contractTerm)
        {
            var user = svc.GetUser(User.Identity.GetUserId());
            var orderUrl = new UrlHelper(HttpContext.Request.RequestContext).Action("Edit", "Order", new {area = "ISPAdmin"}, Request.Url.Scheme);
            bool orderResult = false;

            //Process should not get this far but double check user is in Zone and can order
            if (user.Zone != null && user.Zone.AllowOrder)
            {
                orderResult = svc.PlaceOrder(user.Id, prodId, orderUrl, contractTerm);
                var product = svc.GetProduct(prodId);
                var cap = product.IsCapped ? product.Cap.ToString() : "";
                var isCapped = product.IsCapped ? "Capped" : "Uncapped";

                var email = new EmailDto
                {
                    Subject = "Order confirmation",
                    Body = "Dear " + user.FirstName + "," +
                    "<br/><br/>Your order for " + product.ISP.Name + " " + product.LineSpeed.GetDisplayName() + " " +
                    isCapped + " " + cap + " has been received." +
                    "<br/><br/>If you have any queries then please contact <strong>" + product.ISP.Name +
                    "</strong> at " + product.ISP.EmailAddress + " or on " + product.ISP.LandlineNo + "." +
                    "<br/><br/>Warm regards" +
                    "<br/><br/>The Frogfoot team."
                };

                EmailSender.SendEmail(email, user);
            }

            return RedirectToAction("OrderResult", new { user.Id, ispId, orderResult});
        }

        public ActionResult OrderResult(string userId, bool orderResult, int ispId)
        {
            var model = new OrderResultViewModel
            {
                User = svc.GetUser(User.Identity.GetUserId()),
                ISP = svc.GetISP(ispId),
                OrderResult = orderResult
            };

            return View(model);
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        public JsonResult SaveISPContacts(int[] ids)
        {
            if (ids != null && ids.Length > 0)
            {
                svc.SaveClientISPContacts(ids, User.Identity.GetUserId());
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
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

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }
    }
}