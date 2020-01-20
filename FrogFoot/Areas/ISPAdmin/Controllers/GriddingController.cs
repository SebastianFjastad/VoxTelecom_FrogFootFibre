using System.Web.Mvc;
using FrogFoot.Areas.ISPAdmin.Models;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class GriddingController : Controller
    {
        private ISPService svc = new ISPService();

        public ActionResult Index()
        {
            var user = svc.GetISPUser(User.Identity.GetUserId());
            var model = new GriddingViewModel
            {
                ISPId = user.ISPId ?? 0
            };
            return View(model);
        }

        public ActionResult ProductLocationGridding(int ispId)
        {
            var model = new GriddingViewModel
            {
                Locations = svc.GetLocations(),
                ISPProducts = svc.GetProducts(null, ispId),
                ISPId = ispId
            };
            return PartialView(model);
        }

        public ActionResult ProductEstateGridding(int locId, int ispId)
        {
            var model = new GriddingViewModel
            {
                Estates = svc.GetEstatesAndLocations(locId),
                ISPProducts = svc.GetProducts(null, ispId)
            };

            model.ISPId = ispId;
            return PartialView(model);
        }

        public ActionResult UpdateProductLocationGridding(int prodId, int locId, int ispId, int? prodGridId)
        {
            var gridId = svc.UpdateProductLocationGridding(prodId, locId, ispId, prodGridId);
            return Json(new { gridId = gridId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateProductEstateGridding(int prodId, int estId, int ispId, int? prodGridId)
        {
            var gridId = svc.UpdateProductEstateGridding(prodId, estId, ispId, prodGridId);
            return Json(new { gridId = gridId }, JsonRequestBehavior.AllowGet);
        }
    }
}