using System.Web.Mvc;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.Home.Controllers
{
    public class PackagesController : Controller
    {
        private ClientService svc = new ClientService();

        public ActionResult Index()
        {
            var precinctCode = Request.Cookies["PrecinctCode"] != null ? Request.Cookies["PrecinctCode"].Value : "";

            var model = new PackageViewModel
            {
                Products = svc.GetProductsForPublic(precinctCode),
                Suburbs = svc.GetLocations()
            };

            return View(model);
        }

        public ActionResult Product(int prodId, ContractTerm? contractTerm, bool? isTwoOptions)
        {
            ViewBag.ContractTerm = contractTerm;
            ViewBag.IsTwoOptions = isTwoOptions;
            var model = svc.GetProduct(prodId);
            return View(model);
        }
    }
}