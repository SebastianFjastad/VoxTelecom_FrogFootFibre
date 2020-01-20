using System.Web.Mvc;
using FrogFoot.Areas.Client.Models;
using FrogFoot.Services;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.Client.Controllers
{
    [Authorize(Roles = "Client")]
    public class OrdersController : Controller
    {
        private ClientService svc = new ClientService();

        public ActionResult Index()
        {
            var model = new OrdersViewModel
            {
                Orders = svc.GetClientOrders(User.Identity.GetUserId()),
                User = svc.GetUser(User.Identity.GetUserId())
            };

            return View(model);
        }
    }
}