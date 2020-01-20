using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ZoneController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = new ZoneViewModel
            {
                Zones = svc.GetZones(),
                Precincts = svc.GetPrecincts()
            };
            return View(model);
        }

        public ActionResult Create()
        {
            var model = new ZoneViewModel { Precincts = svc.GetPrecincts()};
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ZoneViewModel model)
        {
            svc.CreateZone(model.Zone);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int zoneId)
        {
            var model = new ZoneViewModel
            {
                Precincts = svc.GetPrecincts(),
                Zone = svc.GetZone(zoneId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(ZoneViewModel model)
        {
            svc.UpdateZone(model.Zone);
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int zoneId)
        {
            svc.DeleteZone(zoneId);
            return RedirectToAction("Index");
        }

        public ActionResult SyncPrecinct(string precinctCode)
        {
            ZoneSync.ProcessUsers(precinctCode);
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
    }
}