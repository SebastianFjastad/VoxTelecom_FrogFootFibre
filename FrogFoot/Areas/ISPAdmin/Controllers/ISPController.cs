using System.Web.Mvc;
using FrogFoot.Services;

namespace FrogFoot.Areas.ISPAdmin.Controllers
{
    [Authorize(Roles = "ISPUser")]
    public class ISPController : Controller
    {
        private ISPService svc = new ISPService();

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Order");
        }

        public ActionResult Locations()
        {
            var model = svc.GetLocations();
            return View(model);
        }
    }
}