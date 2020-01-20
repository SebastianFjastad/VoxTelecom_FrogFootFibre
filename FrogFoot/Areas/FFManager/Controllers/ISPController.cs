using System.Web.Mvc;
using FrogFoot.Services;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class ISPController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult Index()
        {
            return View(svc.GetISPs());
        }

        public ActionResult Products(int id)
        {
            return View(svc.GetProducts(id));
        }
    }
}