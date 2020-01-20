using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FrogFoot.Areas.FFUser.Controllers
{
    [Authorize(Roles = "FFUser")]
    public class CoverageController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}