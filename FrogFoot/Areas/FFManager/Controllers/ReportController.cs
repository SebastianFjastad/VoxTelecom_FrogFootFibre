using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class ReportController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult SalesReport()
        {
            var model = svc.GetSalesReport();
            return View(model);
        }

        public ActionResult InterestReport()
        {
            var model = svc.GetUsersWithOrderStatus();
            return View(model);
        }

        public ActionResult UsersInZone()
        {
            var model = new UsersInZoneViewModel
            {
                Zones = svc.GetZones(),
                Precincts = svc.GetPrecincts()
            };
            return View(model);
        }

        public ActionResult ExportUsersInZoneReport(string precinctCode, int? zoneId)
        {
            var users = svc.GetUsers().Include(u => u.Orders)
                                .Where(u => u.LocationId != null && u.Location.PrecinctCode == precinctCode && (zoneId == null || (zoneId != null && zoneId == u.ZoneId))).ToList();
            return ExcelBuilder.GetUsersInZoneReport(users);
        }

        public ActionResult ExportUsersReportToExcel()
        {
            var model = svc.GetUsersReport();
            return ExcelBuilder.GetUsersReport(model);
        }

        public ActionResult ExportISPUsersReportToExcel()
        {
            var model = svc.GetISPUsersData();
            return ExcelBuilder.GetISPUsersReport(model);
        }

        public ActionResult ExportInterestReportToExcel()
        {
            var uiModel = svc.GetUsersWithOrderStatus();
            var reportModel = svc.GetSalesReport();
            return ExcelBuilder.GetInterestReport(uiModel, reportModel);
        }

        public ActionResult ExportSalesReportToExcel()
        {
            var model = svc.GetSalesReport();
            return ExcelBuilder.BuildSalesReport(model);
        }
    }
}