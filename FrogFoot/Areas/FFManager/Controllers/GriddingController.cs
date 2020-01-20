using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.FFManager.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class GriddingController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Suburbs()
        {
            var model = new GriddingViewModel
            {
                Locations = svc.GetLocations()
            };

            return View(model);
        }

        public ActionResult Estates()
        {
            var model = new GriddingViewModel
            {
                Estates = svc.GetEstates()
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

        public ActionResult Gridding()
        {
            var model = new Admin.Models.GriddingViewModel {ISPs = svc.GetISPs() };;
            return View(model);
        }

        public ActionResult ProductLocationGridding(int ispId)
        {
            var model = new Admin.Models.GriddingViewModel
            {
                Locations = svc.GetLocations(),
                ISPProducts = svc.GetProducts(ispId)
            };
            return PartialView(model);
        }

        public ActionResult ProductEstateGridding(int locId, int ispId)
        {
            var model = new Admin.Models.GriddingViewModel
            {
                Estates = svc.GetEstatesAndLocation(locId),
                ISPProducts = svc.GetProducts(ispId)
            };
            return PartialView(model);
        }
    }
}