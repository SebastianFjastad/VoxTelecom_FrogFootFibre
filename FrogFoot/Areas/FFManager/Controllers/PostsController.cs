using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.FFManager.Controllers
{

    public class PostsController : Controller
    {
        private AdminService svc = new AdminService();

        [Authorize(Roles = "FFManager, Comms")]
        public ActionResult Index()
        {
            var model = svc.GetPosts();
            return View(model);
        }

        [Authorize(Roles = "Comms")]
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
        [Authorize(Roles = "Comms")]
        public ActionResult Create(PostViewModel model, HttpPostedFileBase upload)
        {
            svc.CreatePost(model.Post, upload);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Comms")]
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
        [Authorize(Roles = "Comms")]
        public ActionResult Edit(PostViewModel model, HttpPostedFileBase upload)
        {
            svc.EditPost(model.Post, upload);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Comms")]
        public ActionResult Delete(int postId)
        {
            svc.DeletePost(postId);
            return RedirectToAction("Index");
        }
    }
}