using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Client.Models;
using FrogFoot.Models;
using FrogFoot.Models.Datatables;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;
using PostViewModel = FrogFoot.Areas.Admin.Models.PostViewModel;

namespace FrogFoot.Areas.Client.Controllers
{
    [Authorize(Roles = "Champ")]
    public class FibreChampController : Controller
    {
        private FibreChampService svc = new FibreChampService();

        public ActionResult Posts()
        {
            var model = svc.GetPosts(User.Identity.GetUserId());
            return View(model);
        }

        public ActionResult Create()
        {
            var model = svc.GetPostCreateData(User.Identity.GetUserId());
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(PostViewModel model)
        {
            svc.CreatePost(model.Post, model.Post.PrecinctCode);
            return RedirectToAction("Posts");
        }

        public ActionResult Edit(int postId)
        {
            var model = svc.GetPostCreateData(User.Identity.GetUserId());
            model.Post = svc.GetPost(postId);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PostViewModel model)
        {
            svc.EditPost(model.Post);
            return RedirectToAction("Posts");
        }

        public ActionResult Delete(int postId)
        {
            svc.DeletePost(postId);
            return RedirectToAction("Posts");
        }

        public ActionResult UsersMap()
        {
            var userId = User.Identity.GetUserId();
            var model = new UserMapViewModel
            {
                User = svc.GetUser(userId),
                Users = svc.GetUsers(userId).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public JsonResult UserDataHandler(DTParameters param)
        {
            try
            {
                //need to proeject the users returned into UsersDto
                var dtsource = svc.GetUsers(User.Identity.GetUserId())
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        Landline = u.Landline,
                        Address = u.Address,
                        ZoneObj = u.Zone,
                        OrdersObj = u.Orders,
                    }).ToList();

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