using System;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class UserController : Controller
    {
        private ISPService svc = new ISPService();
        private UserManager _userManager;

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

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var model = svc.GetISPUsers(userId);
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(User model)
        {
            //get the ISP Id of the creating user to use to create the other user
            var user = svc.GetISPUser(User.Identity.GetUserId());

            var existingUser = UserManager.FindByEmail(model.Email);
            if (existingUser != null)
            {
                ViewBag.UserExistsMessage = "The user with email " + model.Email + " already exists in the system.";
                return View();
            }

            model.EmailConfirmed = true;
            model.ISPId = user.ISPId;
            model.UserName = model.Email;
            model.CreatedDate = DateTime.Now;
            IdentityResult x = UserManager.Create(model, "password123");
            UserManager.AddToRole(model.Id, UserType.ISPUser.ToString());


            TempData["Notification"] = "User successfully created";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string userId)
        {
            var model = svc.GetISPUser(userId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(User model)
        {
            svc.EditUser(model);
            TempData["Notification"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(string userId)
        {
            svc.DeleteUser(userId);
            TempData["Notification"] = "User deleted successfully.";
            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword(string userId)
        {
            string code = UserManager.GeneratePasswordResetToken(userId);
            var callbackUrl = Url.Action("ResetPassword", "Account", new {area = "Home", userId = userId, code = code }, protocol: Request.Url.Scheme);

            var user = svc.GetISPUser(userId);

            var email = new EmailDto
            {
                Subject = "Frogfoot account reset",
                Body =
                    "Your account has been reset by your account manager. Please click " +
                    "<a href=\"" + callbackUrl + "\">here</a> to choose a new password and log in."+
                    "<br/><br/> Warm regards," +
                    "<br/><br/> The Frogfoot Fibre team"
            };

            var message = "Account successfuly reset. User has been emailed their new credentials.";
            EmailSender.SendEmail(email, user);

            return Json(new { message }, JsonRequestBehavior.AllowGet);

        }
    }
}