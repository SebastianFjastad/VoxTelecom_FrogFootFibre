using System;
using System.Web.Mvc;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManageISPController : Controller
    {
        private AdminService svc = new AdminService();

        public ActionResult ISP(int ispId)
        {
            var model = new ISPViewModel { ISP = svc.GetISP(ispId)};
            return View(model);
        }

        public ActionResult AddISP()
        {
            return View();
        }

        public ActionResult ISPs()
        {
            return View(svc.GetISPs());
        }

        [HttpPost]
        public ActionResult SaveISP(ISPViewModel model)
        {
            var response = svc.SaveISP(model.ISP);
            ViewBag.Response = response.Message;

            return RedirectToAction("ISPs");
        }

        public ActionResult DeleteISP(int ispId)
        {
            var response = svc.DeleteISP(ispId);
           
            return RedirectToAction("ISPs");
        }

        public ActionResult LeadsAudit()
        {
            return View();
        }

        public PartialViewResult FindUserForISPAudit(string email, string cellNo, string landline)
        {
            var model = svc.GetUserByParams(email, cellNo, landline);
            return PartialView(model);
        }
    }
}