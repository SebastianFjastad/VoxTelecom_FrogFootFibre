using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Models;
using FrogFoot.Models.Datatables;
using FrogFoot.Services;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private AdminService svc = new AdminService();
 
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UsersMap()
        {
            var model = svc.GetUserMapUsers();
            return View(model);
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
                    param.Start, param.Length, 
                    dtsource, columnSearch, 
                    UserFilterType.UserMap).ToList();

                int count = new ResultSet().Count(param.Search.Value, dtsource, columnSearch, UserFilterType.UserMap);
                DTResult<UserDto> result = new DTResult<UserDto>
                {
                    draw = param.Draw,
                    data = data.ToList(),
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