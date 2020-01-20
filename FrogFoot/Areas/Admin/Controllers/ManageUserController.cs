using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Models;
using FrogFoot.Models.Datatables;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageUserController : Controller
    {
        private AdminService svc = new AdminService();
        private ApplicationSignInManager _signInManager;
        private UserManager _userManager;

        public ManageUserController()
        {
            _userManager = new UserManager(new UserStore<User>(new ApplicationDbContext()));
        }

        public ManageUserController(UserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
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

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddUser()
        {
            var model = new UserViewModel { ISPList = svc.GetISPs(), Locations = svc.GetLocations(), Estates = svc.GetEstates()};
            return View(model);
        }

        [HttpPost]
        public ActionResult AddUser(UserViewModel userParam, UserType type)
        {
            //if the user is of type Admin, FFUser, FFManager, Comms, then confirm email automatically
            var emailConfirmed = type != UserType.Client && type != UserType.ISPUser;

            var password = "password123";
            if (type != UserType.FFUser && type != UserType.Admin && type != UserType.FFManager && type != UserType.Comms)
            {
                password = PasswordGenerator.Generate(6);
            }

            var model = new AdminRegisterViewModel
            {
                Email = userParam.User.Email,
                EmailConfirmed = emailConfirmed,
                Password = password,
                ConfirmPassword = password,
                FirstName = userParam.User.FirstName,
                LastName = userParam.User.LastName,
                PhoneNumber = userParam.User.PhoneNumber,
                LocationId = userParam.User.LocationId,
                EstateId = userParam.User.EstateId,
                Address = userParam.User.Address,
                Latitude = userParam.User.Latitude,
                Longitude = userParam.User.Longitude,
                Type = type,
                ISPId = userParam.User.ISPId,
                IsChamp = userParam.User.IsChamp,
                IsUserTypeAdmin = userParam.User.IsUserTypeAdmin,
                UsePrecinctCodeForChamp = userParam.User.UsePrecinctCodeForChamp
            };

            TempData["userModel"] = model;
            return RedirectToAction("AdminRegister", "Account", new { area = "Home" });
        }

        public ActionResult EditUser(string userId)
        {
            var model = new UserViewModel{ User = svc.GetUser(userId), Locations = svc.GetLocations(), Estates = svc.GetEstates()};
            return View(model);
        }

        [HttpPost]
        public ActionResult EditUser(UserViewModel model)
        {
            if (!model.HasErrors)
            {
                //update password if the password string is not empty
                if (!string.IsNullOrEmpty(model.UserPassword))
                {
                    var user = _userManager.FindByEmail(model.User.Email);
                    user.PasswordHash = _userManager.PasswordHasher.HashPassword(model.UserPassword);
                    _userManager.Update(user);
                    var email = new EmailDto();
                    email.Subject = "Password reset";
                    email.Body = "Hi " + user.FirstName + "," +
                                 "<br/><br/>Your password has been reset by the Frogfoot Admin. Your credentials are as follows: " +
                                 "<br/><br/><strong>Username: </strong> " + user.Email +
                                 "<br/><strong>Password: </strong> " + model.UserPassword +
                                 "<br/><br/>Kind regards, " +
                                 "<br/><br/>The Frogfoot Fibre team";
                    EmailSender.SendEmail(email, user);
                }

                svc.EditUser(model.User);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult DeleteUser(string id)
        {
            var result = svc.DeleteUser(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult UserDataHandler(DTParameters param)
        {
            try
            {
                var dtsource = svc.GetUserDtos(u => !u.IsDeleted).AsNoTracking().ToList();
                List<string> columnSearch = new List<string>();
                foreach (var col in param.Columns)
                {
                    columnSearch.Add(col.Search.Value);
                }

                List<UserDto> data = new ResultSet().GetResult<IEnumerable<UserDto>>(
                    param.Search.Value, 
                    param.SortOrder, 
                    param.Start, 
                    param.Length, 
                    dtsource, 
                    columnSearch, 
                    UserFilterType.UserTable).ToList();

                var manager = new UserManager(new UserStore<User>(Db.GetInstance()));

                foreach (var user in data)
                {
                    IList<string> roles = manager.GetRoles(user.Id);
                    if (roles.Contains("ISPUser") && user.ISPObj != null)
                    {
                        user.ISPName = user.ISPObj.Name;
                        user.Role = "ISPUser";
                    }
                    else if (roles.Contains("Champ"))
                    {
                        if (user.UsePrecinctCodeForChamp || user.EstateObj == null)
                        {
                            user.ChampCoverage = user.LocationObj.Name;
                        }
                        else
                        {
                            user.ChampCoverage = string.Format("<strong>{0}</strong> in {1}", user.EstateObj.Name,
                                user.LocationObj.Name);
                        }
                        user.Role = "Champ";
                    }
                    else
                    {
                        user.Role = roles.FirstOrDefault();
                    }

                    //for circular json serialization problem
                    user.EstateObj = null;
                    user.LocationObj = null;
                    user.ZoneObj = null;
                    user.OrdersObj = null;
                    user.ISPClientContactObj = null;
                    user.ClientContactMethods = null;
                }

                int count = new ResultSet().Count(param.Search.Value, dtsource, columnSearch, UserFilterType.UserTable);
                DTResult<UserDto> result = new DTResult<UserDto>
                {
                    draw = param.Draw,
                    data = data,
                    recordsFiltered = count,
                    recordsTotal = count
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}