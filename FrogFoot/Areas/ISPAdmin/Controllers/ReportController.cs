using System;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using FrogFoot.Areas.ISPAdmin.Models;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class ReportController : Controller
    {
        private ISPService svc = new ISPService();

        public ActionResult Index()
        {
            var model = new ReportViewModel { Locations = svc.GetLocations() };
            return View(model);
        }

        [HttpPost]
        public ActionResult Report(ReportViewModel model)
        {
            model.Reports = svc.GetReports(model, User.Identity.GetUserId());
            model.Locations = svc.GetLocations();
            return View(model);
        }

        [HttpPost]
        public void ExportSalesReportToExcel(int? locationId, DateTime? from, DateTime? to)
        {
            var model = new ReportViewModel
            {
                LocationId = locationId,
                From = from,
                To = to
            };

            var grid = new System.Web.UI.WebControls.GridView();
            grid.DataSource = svc.GetReports(model, User.Identity.GetUserId());
            grid.DataBind();
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=OrderReport.xls");
            Response.ContentType = "application/ms-excel";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.RenderControl(htw);
            Response.Write(sw.ToString());
            Response.End();
        }
    }
}