using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using OrderViewModel = FrogFoot.Areas.ISPAdmin.Models.OrderViewModel;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class OrderController : Controller
    {
        private ISPService svc = new ISPService();

        public ActionResult Index()
        {
            var model = new OrderViewModel
            {
                Orders = svc.GetOrders(User.Identity.GetUserId())
            };

            //make sure the back button on the edit page reloads the index page.
            Response.Cache.SetNoStore();

            return View(model);
        }

        public ActionResult Create()
        {
            var model = svc.GetDataForOrderCreate(User.Identity.GetUserId());
            return View(model);
        }

        [HttpPost]
        public ActionResult Create(OrderViewModel model)
        {
            //if user's zone is not able to order, return with error
            if (!ZoneSync.CheckUserZoneForISPOrdering(model.Order.Client))
            {
                TempData["IsAbleToOrderForUser"] = false;
                return RedirectToAction("Create");
            }

            var orderUrl = new UrlHelper(HttpContext.Request.RequestContext).Action("Edit", "Order", new { area = "ISPAdmin" }, Request.Url.Scheme);
            var savedOrder = svc.SaveOrder(model, "ISPUser", orderUrl, User.Identity.GetUserId());
            return RedirectToAction("Details", new {id = savedOrder.OrderId});
        }

        public ActionResult Edit(int id)
        {
            var model = svc.GetDataForOrderCreate(User.Identity.GetUserId());
            model.Order = svc.GetOrder(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int orderId, int ffProductId, int ispProductId, ContractTerm clientContractTerm, List<FFProdEditDto> ffProducts)
        {
            svc.EditOrder(orderId, ffProductId, ispProductId, User.Identity.GetUserId(), clientContractTerm, ffProducts);
            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditClient(OrderViewModel model)
        {
            svc.EditClient(model.Order.Client);
            TempData["ClientUpdated"] = true;
            return RedirectToAction("Edit", new { id = model.Order.OrderId});
        }

        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var model = svc.GetOrder(id);
            return View(model);
        }

        public ActionResult ClientSearch(string term)
        {
            var results = svc.SearchClient(term);
            var model = results.Select(r => new { label = r.Email, value = r.Id }).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetClient(string clientId)
        {
            var model = svc.GetClient(clientId);
            //for serializing issue
            model.Orders = null;
            model.Location = null;
            model.Estate = null;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetUserOrderStatus(string email, decimal? lat, decimal? lng)
        {
            ClientOrderStatusDto result = svc.GetUserOrderStatus(email, lat, lng);
            if (result.Client != null)
            {
                result.Client.Location = null;
                result.Client.Estate = null;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateStatus(int orderId, OrderStatus status)
        {
            svc.UpdateStatus(orderId, status, User.Identity.GetUserId());
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveISPOrderNo(int orderId, string ispOrderNo)
        {
            svc.SaveISPOrderNo(orderId, ispOrderNo, User.Identity.GetUserId());
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IncludedProducts(int locId, int? estateId, int? ispId)
        {
            var products = svc.GetGriddedProducts(locId, estateId, ispId);
            return PartialView(products);
        }

        public ActionResult DeleteOrder(int id)
        {
            svc.CancelOrder(id, User.Identity.GetUserId());
            return RedirectToAction("Index");
        }

        public FileResult ViewPDF(int id)
        {
            var pdf = svc.GetPDF(id);
            return File(pdf.AssetPath, "application/pdf");
        }

        public FileResult DownloadPDF(int id)
        {
            var pdf = svc.GetPDF(id);
            return File(pdf.AssetPath, "application/pdf");
        }

        public JsonResult SaveMessage(int orderId, string message)
        {
            var response = svc.SaveMessage(orderId, message, User.Identity.GetUserId());
            if (response.User != null)
            {
                response.User.ISPClientContacts = null;
                response.User.ClientContactMethods = null;
                response.User.ClientISPContacts = null;
            }
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}