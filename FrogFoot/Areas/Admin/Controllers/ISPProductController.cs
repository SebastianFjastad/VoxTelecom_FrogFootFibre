using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.ISPAdmin.Models;
using FrogFoot.Repositories;
using ISPProductViewModel = FrogFoot.Areas.Admin.Models.ISPProductViewModel;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ISPProductController : Controller
    {
        private ISPRepository repo = new ISPRepository();

        public ActionResult Index()
        {
            return View(repo.GetISPsForProducts());
        }

        public ActionResult Products(int ispId, string ispName = "")
        {
            var model = new ISPProductViewModel
            {
                ISPId = ispId,
                ISPName = ispName,
                ISPProducts = repo.GetProductsForISP(ispId)
            };

            ModelState.Clear();
            return View(model);
        }

        public ActionResult CreateProduct(int ispId, string ispName = "")
        {
            var model = new ISPProductViewModel
            {
                ISPId = ispId,
                ISPName = ispName
            };

            return View(model);
        }

        public ActionResult SaveProduct(ISPProductViewModel model, HttpPostedFileBase upload)
        {
            if (ModelState.IsValid)
            {
                var result = repo.SaveProduct(model.ISPProduct, upload);
                return RedirectToAction("Index");
            }
            return RedirectToAction("CreateProduct");
        }

        public ActionResult EditProduct(int ispId, int prodId)
        {
            var model = new ISPProductViewModel
            {
                ISPId = ispId,
                ISPProduct = repo.GetProduct(prodId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ISPProductViewModel model, HttpPostedFileBase upload)
        {
            repo.EditProduct(model.ISPProduct, upload);
            return RedirectToAction("Products", new {ispId = model.ISPId});
        }

        public ActionResult SaveProductsStatus(List<ProductActiveDto> products)
        {
            repo.SaveProductsStatus(products);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteProduct(int prodId)
        {
            repo.DeleteProduct(prodId);
            return RedirectToAction("Index");
        }
    }
}