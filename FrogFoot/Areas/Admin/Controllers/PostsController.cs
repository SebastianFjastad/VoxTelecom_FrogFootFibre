using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PostsController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = svc.GetPosts();
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new PostViewModel
            {
                Precincts = svc.GetPrecincts(),
                Zones = svc.GetZones()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(PostViewModel model, HttpPostedFileBase upload)
        {
            svc.CreatePost(model.Post, upload);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int postId)
        {
            var model = new PostViewModel
            {
                Precincts = svc.GetPrecincts(),
                Post = svc.GetPost(postId),
                Zones = svc.GetZones()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PostViewModel model, HttpPostedFileBase upload)
        {
            svc.EditPost(model.Post, upload);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int postId)
        {
            svc.DeletePost(postId);
            return RedirectToAction("Index");
        }
    }
}