using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            var model = svc.GetSalesReport();
            return View(model);
        }

        public ActionResult ExportSalesReportToExcel()
        {
            var model = svc.GetSalesReport();
            return ExcelBuilder.BuildSalesReport(model);
        }

        public ActionResult ExportUsersReportToExcel()
        {
            var model = svc.GetUsersReport();
            return ExcelBuilder.GetUsersReport(model);
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
            var usersInZone = svc.GetUsers().Include(u => u.Orders)
                .Where(u => u.LocationId != null && u.Location.PrecinctCode == precinctCode && (zoneId == null || (zoneId != null && zoneId == u.ZoneId))).ToList();
            return ExcelBuilder.GetUsersInZoneReport(usersInZone);
        }

        public ActionResult ExportInterestReportToExcel()
        {
            var uiModel = svc.GetUsersWithOrderStatus();
            var reportModel = svc.GetSalesReport();
            return ExcelBuilder.GetInterestReport(uiModel, reportModel);
        }

        public ActionResult ExportISPUsersReportToExcel()
        {
            var model = svc.GetISPUsersData();
            return ExcelBuilder.GetISPUsersReport(model);
        }

    }
}