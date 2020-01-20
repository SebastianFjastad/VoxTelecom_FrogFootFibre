using System.Web.Mvc;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.Home.Controllers
{
    public class NewsController : Controller
    {
        private ClientService svc = new ClientService();

        public ActionResult Index()
        { 
             var model = new NewsViewModel
             {
                 Posts = svc.GetPosts(User.Identity.GetUserId(), PostType.Article),
             };
            return View(model);
        }

        public ActionResult NewsPage(PostType? type, int[] postIdsToExclude, int pageNo = 1, int pageSize = 5)
        {
            var model = svc.GetPosts(null, type, postIdsToExclude, pageNo, pageSize);
            return PartialView(model);
        }

        public ActionResult Post(int postId)
        {
            var model = new NewsViewModel
            {
                Post = svc.GetPost(postId),
                Posts = svc.GetPosts(null, null, new[] { postId })
            };
            return View(model);
        }
    }
}