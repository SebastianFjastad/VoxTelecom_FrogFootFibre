using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.ISPAdmin.Models;
using FrogFoot.Entities;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class ProductsController : Controller
    {
        private ISPService svc = new ISPService();

        public ActionResult Index()
        {
            var user = svc.GetISPUser(User.Identity.GetUserId());

            //if user isnt user type admin return to login screen
            if (!user.IsUserTypeAdmin) return RedirectToAction("Login", "Account");

            //set cookie for ISP Admin
            var userCookie = new HttpCookie("ISPAdmin", user.Id);
            userCookie.Expires.AddDays(1);
            HttpContext.Response.SetCookie(userCookie);

            var model = svc.GetProducts(user.Id, null);

            return View(model);
        }

        public ActionResult Details(int prodId)
        {
            if (Request.Cookies["ISPAdmin"] == null)
                return RedirectToAction("Login", "Account");

            return View(svc.GetProduct(prodId));
        }

        public ActionResult Create()
        {
            if (Request.Cookies["ISPAdmin"] == null)
                return RedirectToAction("Login", "Account");

            var user = svc.GetISPUser(User.Identity.GetUserId());
            var model = new ISPProductViewModel
            {
                ISPProduct = new ISPProduct {ISPId = user.ISPId ?? 0}
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(ISPProductViewModel model, HttpPostedFileBase upload)
        {
            svc.SaveProduct(model.ISPProduct, upload);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int prodId)
        {
            if (Request.Cookies["ISPAdmin"] == null)
                return RedirectToAction("Login", "Account");

            return View(svc.GetProduct(prodId));
        }

        [HttpPost]
        public ActionResult Edit(ISPProduct model, HttpPostedFileBase upload)
        {
            if (Request.Cookies["ISPAdmin"] == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                svc.EditProduct(model, upload);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SaveProductsStatus(List<ProductActiveDto> products)
        {
            svc.SaveProductsStatus(products);
            return Json(new { success = true}, JsonRequestBehavior.AllowGet );
        }

        public ActionResult Delete(int prodId, int ispId)
        {
            var userId = User.Identity.GetUserId();
            svc.DeleteProduct(userId, prodId, ispId);
            return RedirectToAction("Index");
        }
    }
}