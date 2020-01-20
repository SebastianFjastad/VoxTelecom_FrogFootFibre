using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Models;
using FrogFoot.Models.Datatables;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class LeadsController : Controller
    {
        private ISPService svc = new ISPService();
       
        public ActionResult Index()
        {
            var ispUser = svc.GetISPUser(User.Identity.GetUserId());
            return View(ispUser.ISP);
        }

        [HttpPost]
        public JsonResult ClientDataHandler(DTParameters param)
        {
            try
            {
                var ispUser = svc.GetISPUser(User.Identity.GetUserId());
                //if ISP user id allowed to see the leads
                if (ispUser.ISP != null && ispUser.ISP.AllowViewLeads)
                {
                    var rnd = new Random(ispUser.ISPId ?? 0); //ispId will never be null

                    var dtsource = svc.GetUserDtos(u =>
                    !u.IsDeleted 
                    && (u.Zone != null && u.Zone.AllowLeads)
                    && u.ISPCommsOptOutStatus == false
                    && u.ISPClientContacts.All(c => c.ISPId != ispUser.ISPId)
                    && !u.Orders.Any()
                    && u.ClientISPContacts.Any(c => c.ISPId == ispUser.ISPId)
                    && u.Location.ISPLocationProducts.Any(il => il.ISPId == ispUser.ISPId)
                    && (u.Estate == null || u.Estate != null && u.Estate.ISPEstateProducts.Any(ie => ie.ISPId == ispUser.ISPId)))
                    .ToList().Shuffle(rnd).ToList();
                   
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
                        UserFilterType.UserLeads).ToList();

                    int count = new ResultSet().Count(param.Search.Value, dtsource, columnSearch, UserFilterType.UserLeads);

                    DTResult<UserDto> result = new DTResult<UserDto>
                    {
                        draw = param.Draw,
                        data = data,
                        recordsFiltered = count,
                        recordsTotal = count
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(new {});
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult LeadsDataHandler(DTParameters param)
        {
            try
            {
                var ispUser = svc.GetISPUser(User.Identity.GetUserId());
                //if ISP user id allowed to see the leads
                if (ispUser.ISP != null && ispUser.ISP.AllowViewLeads)
                {
                    var dtsource = svc.GetUserDtos(u => 
                    !u.IsDeleted
                    && u.Zone.AllowLeads 
                    && u.ISPCommsOptOutStatus == false
                    && u.ClientISPContacts.Any(c => c.ISPId == ispUser.ISPId)
                    && u.ISPClientContacts.Any(i => i.ISPId == ispUser.ISPId)
                    , 
                    ispUser.ISPId)
                    .ToList();
                   
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
                        UserFilterType.UserLeads).ToList();

                    int count = new ResultSet().Count(param.Search.Value, dtsource, columnSearch, UserFilterType.UserLeads);

                    DTResult<UserDto> result = new DTResult<UserDto>
                    {
                        draw = param.Draw,
                        data = data,
                        recordsFiltered = count,
                        recordsTotal = count
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(new {});
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        public JsonResult CreateISPClientContact(string clientId, int ispId)
        {
            svc.CreateISPClientContact(User.Identity.GetUserId(), clientId, ispId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MakeISPContact(string clientId, int ispId)
        {
            svc.MakeISPContact(clientId, ispId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}