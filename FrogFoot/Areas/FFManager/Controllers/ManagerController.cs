using System.Web.Mvc;

namespace FrogFoot.Areas.FFManager.Controllers
{
    [Authorize(Roles = "FFManager,Comms")]
    public class ManagerController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}