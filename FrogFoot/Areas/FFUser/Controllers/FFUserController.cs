using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrogFoot.Areas.FFUser.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Rotativa;

namespace FrogFoot.Areas.FFUser.Controllers
{
    [Authorize(Roles = "FFUser")]
    public class FFUserController : Controller
    {
        private FFUserService svc = new FFUserService();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Orders()
        {
            var model = new OrderViewModel
            {
                Orders = svc.GetOrders()
            };

            //make sure the back button on the edit page reloads the index page.
            Response.Cache.SetNoStore();

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Edit(int id)
        {
            var model = svc.GetOrder(id);
            Response.Cache.SetNoStore();
            return View(model);
        }

        public ActionResult Cancel(int id)
        {
            svc.CancelOrder(id, User.Identity.GetUserId());
            return RedirectToAction("Index");
        }

        public ActionResult UpdateStatus(int orderId, OrderStatus status)
        {
            svc.UpdateOrder(orderId, status, User.Identity.GetUserId());
            CreatePDF(orderId, status);
            EmailSender.SendRTAcceptedOrderPDF(orderId);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
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

            var actionResult = new ActionAsPdf("Edit", new { id = order.OrderId });

            var byteArray = actionResult.BuildPdf(ControllerContext);

            var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
            fileStream.Write(byteArray, 0, byteArray.Length);
            fileStream.Close();
        }
    }
}