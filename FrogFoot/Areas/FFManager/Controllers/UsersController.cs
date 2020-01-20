using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Models;
using FrogFoot.Models.Datatables;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class UsersController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult UserDataHandler(DTParameters param)
        {
            try
            {
                var dtsource = svc.GetUserDtos(u => !u.IsDeleted).ToList();
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