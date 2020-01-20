using System;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;

namespace FrogFoot.Areas.FFUser.Controllers
{
    [Authorize(Roles = "FFUser")]
    public class ReportsController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = new ReportViewModel { Locations = svc.GetLocations() };
            return View(model);
        }

        [HttpPost]
        public ActionResult Report(ReportViewModel model)
        {
            model.Reports = svc.GetAdminSalesReports(model);
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
            grid.DataSource = svc.GetAdminSalesReports(model);
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