using System.Web;
using System.Web.Mvc;
using FrogFoot.Entities;
using FrogFoot.Services;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PortalsController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = svc.GetAllPortals();
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Portal model, HttpPostedFileBase upload)
        {
            svc.SavePortal(model, upload);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = svc.GetPortal(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(Portal model, HttpPostedFileBase upload)
        {
            svc.SavePortal(model, upload);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            svc.DeletePortal(id);
            return RedirectToAction("Index");
        }

        public JsonResult DeleteUrl(int id)
        {
            svc.DeleteUrl(id);
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
    }
}