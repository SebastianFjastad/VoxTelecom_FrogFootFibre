using FrogFoot.Entities;
using System.Web.Mvc;
using FrogFoot.Areas.Client.Models;
using FrogFoot.Models;
using FrogFoot.Repositories;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.Client.Controllers
{
    [Authorize(Roles = "Client")]
    public class NewsController : Controller
    {
        private ClientService svc = new ClientService();

        public ActionResult NewsPage(PostType? type, int[] postIdsToExclude, int pageNo = 1, int pageSize = 5)
        {
            var model = svc.GetPosts(User.Identity.GetUserId(), type, postIdsToExclude, pageNo, pageSize);
            return PartialView(model);
        }

        public ActionResult Post(int postId)
        {
            var model = new PostViewModel
            {
                Post = svc.GetPost(postId),
                Posts = svc.GetPosts(User.Identity.GetUserId(), null, new []{postId}),
                User = svc.GetUser(User.Identity.GetUserId())
            };
            return View(model);
        }

        public ActionResult FibreStatus(Zone model)
        {
            return PartialView(model);
        }
    }
}