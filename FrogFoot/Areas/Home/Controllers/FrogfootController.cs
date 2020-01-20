using System.Web.Mvc;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Services;

namespace FrogFoot.Areas.Home.Controllers
{
    public class FrogfootController : Controller
    {
        private ClientService svc = new ClientService();

        public ActionResult Coverage()
        {
            var model = new CoverageViewModel
            {
                Locations = svc.GetLocations(),
                Estates = svc.GetEstates()
            };
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult WhatIsFTTH()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult TermsOfUse()
        {
            return View();
        }

        public ActionResult FAQ()
        {
            return View();
        }
    }
}