using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Rotativa;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = new OrderViewModel
            {
                Orders = svc.GetOrders()
            };

            //make sure the back button on the edit page reloads the index page.
            Response.Cache.SetNoStore();

            return View(model);
        }

        public ActionResult Create()
        {
            var model = svc.GetDataForOrderCreate();
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
            svc.SaveOrder(model, "Admin", orderUrl, User.Identity.GetUserId());
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var model = svc.GetDataForOrderCreate();
            model.Order = svc.GetOrder(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int orderId, int ffProductId, int ispProductId, OrderStatus orderStatus, ContractTerm clientContractTerm,  List<FFProdEditDto> ffProducts)
        {
            svc.EditOrder(orderId, ffProductId, ispProductId, orderStatus, User.Identity.GetUserId(), clientContractTerm, ffProducts);
            CreatePDF(orderId, orderStatus);
            EmailSender.SendRTAcceptedOrderPDF(orderId);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditClient(OrderViewModel model)
        {
            svc.EditUserFromOrder(model.Order.Client);
            TempData["ClientUpdated"] = true;
            return RedirectToAction("Edit", new { id = model.Order.OrderId });
        }

        public ActionResult ClientSearch(string term)
        {
            var results = svc.SearchClient(term);
            var model = results.Select(r => new { label = r.Email, value = r.Id }).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetClient(string clientId)
        {
            var model = svc.GetUser(clientId);
            //for serializing issue
            model.Orders = null;
            model.Location = null;
            model.Estate = null;
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateStatus(int orderId, OrderStatus status)
        {
            UpdateOrderStatus(orderId, status, User.Identity.GetUserId());
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult ExternalOrderStatusUpdate(string solidId, int orderId, OrderStatus status)
        {
            var solidUserId = ConfigurationManager.AppSettings["SolidUserId"];

            //if the ids don't match then the user isn't the SOLID user
            if (solidId != solidUserId) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            try
            {
                UpdateOrderStatus(orderId, status, solidUserId);
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(500);
            }
        }

        private void UpdateOrderStatus(int orderId, OrderStatus status, string userId)
        {
            svc.UpdateStatus(orderId, status, userId);
            CreatePDF(orderId, status);
            EmailSender.SendRTAcceptedOrderPDF(orderId);
        }

        public ActionResult IncludedProducts(int locId, int? estateId, int? ispId)
        {
            var products = svc.GetGriddedProducts(locId, estateId, ispId);
            return PartialView(products);
        }

        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var model = svc.GetOrder(id);
            return View(model);
        }

        public FileResult ViewPDF(int id)
        {
            var pdf = svc.GetPDF(id);
            return File(pdf.AssetPath, "application/pdf");
        }

        public FileResult DownloadPDF(int id)
        {
            var pdf = svc.GetPDF(id);
            return File(pdf.AssetPath, "application/pdf", pdf.Name);
        }

        public ActionResult Cancel(int id)
        {
            svc.CancelOrder(id, User.Identity.GetUserId());
            return RedirectToAction("Index");
        }

        private void CreatePDF(int orderId, OrderStatus status)
        {
            var order = svc.GetOrder(orderId);

            if (order == null)
            {
                throw new Exception("order not found");
            }

            if (order.ISP == null)
            {
                throw new Exception("ISP is null");
            }

            string fileName = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-") + order.ISP.Name + "-" + status + ".pdf";
            string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/Assets/OrderPDF/");
            string targetPath = Path.Combine(targetFolder, fileName);
            var asset = new Asset { AssetPath = targetPath, Name = fileName, CreatedDate = DateTime.Now };
            svc.SaveOrderPDFAsset(order, asset);
            var actionResult = new ActionAsPdf("Details", new { id = order.OrderId });
            var byteArray = actionResult.BuildPdf(ControllerContext);
            var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();
        }

        public JsonResult SaveMessage(int orderId, string message)
        {
            var response = svc.SaveMessage(orderId, message, User.Identity.GetUserId());
            if (response.User != null)
            {
                response.User.ClientISPContacts = null;
                response.User.ClientContactMethods = null;
                response.User.ISPClientContacts = null;
            }

            return Json(response, JsonRequestBehavior.AllowGet);
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
    }
}