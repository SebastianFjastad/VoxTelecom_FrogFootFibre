using System.Web.Mvc;
using FrogFoot.Services;

namespace FrogFoot.Areas.Client.Controllers
{
    public class SubscriptionController : Controller
    {
        private ClientService svc = new ClientService();

        public ActionResult OptOutISPComms(string id)
        {
            svc.OptOutISPComms(id);
            return View("OptOut");
        }

        public ActionResult OptOutFrogfootComms(string id)
        {
            svc.OptOutFFComms(id);
            return View("OptOut");
        }
    }
}