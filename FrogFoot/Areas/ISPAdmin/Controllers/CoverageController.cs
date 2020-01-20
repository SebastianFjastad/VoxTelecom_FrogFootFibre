using System.Web.Mvc;
using FrogFoot.Services;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class CoverageController : Controller
    {
        private ISPService svc = new ISPService();

        public ActionResult Index()
        {
            var model = svc.GetLocations();
            return View(model);
        }
    }
}