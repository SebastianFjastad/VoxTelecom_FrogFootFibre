using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GriddingController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = new GriddingViewModel {ISPs = svc.GetISPs().ToList()};
            return View(model);
        }

        public ActionResult Suburbs()
        {
            var model = new SettingsViewModel { Locations = svc.GetLocations()};
            return View(model);
        }
        public ActionResult MapAPI()
        {
            var model = new SettingsViewModel { Locations = svc.GetLocations() };
            return View(model);
        }

        public ActionResult Estates()
        {
            var model = new SettingsViewModel
            {
                Estates = svc.GetEstates(),
                Locations = svc.GetLocations()
            };
            return View(model);
        }

        public ActionResult EstateDiscount(int estateId)
        {
            var model = new EstateDiscountViewModel
            {
                Estate = svc.GetEstateDiscounts(estateId),
                ISPs = svc.GetISPs()
            };

            if (model.Estate.ISPEstateDiscounts != null)
            {
                foreach (var ispToRemove in model.Estate.ISPEstateDiscounts.Select(discount => model.ISPs.FirstOrDefault(e => e.ISPId == discount.ISPId)))
                {
                    model.ISPs.Remove(ispToRemove);
                }
            }
            return View(model);
        }

        public ActionResult SaveDiscount(int estateId, int ispId, Discount discount, int? discountId = null)
        {
            svc.SaveDiscount(discountId, estateId, ispId, discount);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveLocation(string name, string apiName, string code, bool active, bool allowOrder, int residents)
        {
            svc.SaveLocation(name, apiName, code, active, allowOrder, residents);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateLocation(int id, string name, string apiName, string code, bool active, bool allowOrder, int residents)
        {
            svc.UpdateLocation(id, name, apiName, code, active, allowOrder, residents);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteLocation(int id)
        {
            svc.DeleteLocation(id);
            return RedirectToAction("Suburbs");
        }

        public ActionResult SaveEstate(int locationId, string name, string code)
        {
            svc.SaveEstate(locationId, name, code);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateEstate(int id, int locationId, string name, string code)
        {
            svc.UpdateEstate(id, locationId, name, code);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteEstate(int id)
        {
            svc.DeleteEstate(id);
            return RedirectToAction("Estates");
        }

        public ActionResult ProductLocationGridding(int ispId)
        {
            var model = new GriddingViewModel {Locations = svc.GetLocations(), ISPProducts = svc.GetProducts(ispId)};
            return PartialView(model);
        }

        public ActionResult ProductEstateGridding(int locId, int ispId)
        {
            var model = new GriddingViewModel
            {
                Estates = svc.GetEstates().Where(e => e.LocationId == locId).ToList(),
                ISPProducts = svc.GetProducts(ispId)
            };
            return PartialView(model);
        }

        public ActionResult UpdateProductLocationGridding(int prodId, int locId, int ispId, int? prodGridId)
        {
            var gridId = svc.UpdateProductLocationGridding(prodId, locId, ispId, prodGridId);
            return Json( new { gridId = gridId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateProductEstateGridding(int prodId, int estId, int ispId, int? prodGridId)
        {
            var gridId = svc.UpdateProductEstateGridding(prodId, estId, ispId, prodGridId);
            return Json( new { gridId = gridId }, JsonRequestBehavior.AllowGet);
        }
    }
}