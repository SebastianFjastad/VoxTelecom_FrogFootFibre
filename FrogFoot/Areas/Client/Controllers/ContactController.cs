using System.Web.Mvc;
using FrogFoot.Areas.Client.Models;
using FrogFoot.Models;
using FrogFoot.Repositories;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.Client.Controllers
{
    [Authorize(Roles = "Client")]
    public class ContactController : Controller
    {
        private UserRepository userRepo = new UserRepository();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(ContactViewModel model)
        {
            EmailSender.SendEmail(new EmailDto { Subject = model.Subject, Body = model.Message}, userRepo.GetUser(User.Identity.GetUserId()), true);
            TempData["EmailResult"] = true;
            return RedirectToAction("Index");
        }
    }
}