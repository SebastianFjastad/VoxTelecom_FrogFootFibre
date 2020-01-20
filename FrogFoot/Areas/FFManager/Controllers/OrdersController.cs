using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;
using Rotativa;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class OrdersController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = new OrderViewModel
            {
                Orders = svc.GetOrders()
            };

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            return View(svc.GetOrder(id));
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
            model.Location.Estates = null; //serializing issue
            model.Estate.Location = null; // serializing issue
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateStatus(int orderId, OrderStatus status)
        {
            svc.UpdateStatus(orderId, status, User.Identity.GetUserId());
            CreatePDF(orderId, status);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult IncludedProducts(int locId, int? estateId, int? ispId)
        {
            var products = svc.GetGriddedProducts(locId, estateId, ispId);
            return PartialView(products);
        }

        public ActionResult Edit(int id)
        {
            var model = svc.GetDataForOrderCreate();
            model.Order = svc.GetOrder(id);
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int orderId, int ffProductId, int ispProductId, OrderStatus orderStatus, ContractTerm clienContractTerm, List<FFProdEditDto> ffProducts)
        {
            svc.EditOrder(orderId, ffProductId, ispProductId, orderStatus, User.Identity.GetUserId(), clienContractTerm, ffProducts);
            CreatePDF(orderId, orderStatus);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditClient(OrderViewModel model)
        {
            svc.EditUserFromOrder(model.Order.Client);
            TempData["ClientUpdated"] = true;
            return RedirectToAction("Edit", new { id = model.Order.OrderId });
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

        public JsonResult SaveMessage(int orderId, string message)
        {
            var response = svc.SaveMessage(orderId, message, User.Identity.GetUserId());
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}