using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Services;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class MapController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Users()
        {
            return View(svc.GetUserMapUsers());
        }

        public ActionResult Coverage()
        {
            var model = svc.GetLocations();
            return View(model);
        }
    }
}