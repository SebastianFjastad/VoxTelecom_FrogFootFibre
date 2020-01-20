using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Services;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;

namespace FrogFoot.Areas.Home.Controllers
{
    public class HomeController : Controller
    {
        private ClientService svc = new ClientService();
        private LoggingService logSvc = new LoggingService();

        public async Task<ActionResult> Index()
        {
            if (Request.IsAuthenticated)
            {
                //all users log in through this method
                //log user log in here
                await logSvc.LogUserLogIn(User.Identity.GetUserId());

                if (User.IsInRole("Client"))
                {
                    return RedirectToAction("Index", "Portal", new { area = "Client" });
                }
                if (User.IsInRole("ISPUser"))
                {
                    return RedirectToAction("Index", "ISP", new { area = "ISPAdmin" });
                }
                if (User.IsInRole("FFUser"))
                {
                    return RedirectToAction("Index", "FFUser", new { area = "FFUser" });
                }
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin", new { area = "Admin" });
                }
                if (User.IsInRole("FFManager") || User.IsInRole("Comms"))
                {
                    return RedirectToAction("Index", "Manager", new { area = "FFManager" });
                }
            }

            //order of precedence for finding the portal

            //first look for URL parameter
            //if no URL param then look for cookie
            //if no cookie then look for URL referrer

            var urlList = svc.GetUrls();
            var portals = svc.GetPortals();
            var locations = svc.GetLocations();

            Portal portal = new Portal();
            bool portalFound = false;
            string precinctCode = "";
            string suburb = "";

            if (Request.RequestContext.RouteData.Values["precinct"] != null)
            {
                suburb = Request.RequestContext.RouteData.Values["precinct"].ToString().ToLower();
            }

            //if there is a Location, find the location and match it's precinct code to the portal's
            if (!string.IsNullOrEmpty(suburb))
            {
                var location = locations.FirstOrDefault(l => l.APIName.ToLower() == suburb);
                if (location != null)
                {
                    portal = portals.FirstOrDefault(p => p.PrecinctCode == location.PrecinctCode);
                    if (portal != null)
                    {
                        portalFound = true;
                    }
                }
            }

            //check if cookie exists and try find portal
            if (!portalFound && Request.Cookies["PrecinctCode"] != null && string.IsNullOrEmpty(suburb))
            {
                precinctCode = Request.Cookies["PrecinctCode"].Value;

                portal = portals.FirstOrDefault(p => p.PrecinctCode == precinctCode);
                if (portal != null)
                {
                    portalFound = true;
                }
            }

            //check if URL contains portal url
            if (!portalFound && Request.Url != null && !string.IsNullOrEmpty(Request.Url.AbsoluteUri))
            {
                var uri = Request.Url.AbsoluteUri.ToLower();
                var urlMatch = urlList.FirstOrDefault(u => !string.IsNullOrEmpty(u.URL) && uri.Contains(u.URL));
                if (urlMatch != null)
                {
                    portal = portals.FirstOrDefault(p => p.PortalId == urlMatch.PortalId);
                    if (portal != null)
                    {
                        portalFound = true;
                    }
                }
            }

            //check if referrer exists and try find portal
            if (!portalFound && Request.UrlReferrer != null && !string.IsNullOrEmpty(Request.UrlReferrer.Host))
            {
                var urlReferrerMatch = urlList.FirstOrDefault(u => string.Equals(u.URL, Request.UrlReferrer.Host.ToString(), StringComparison.CurrentCultureIgnoreCase));

                if (urlReferrerMatch != null)
                {
                    portal = portals.FirstOrDefault(p => p.PortalId == urlReferrerMatch.PortalId);
                    if (portal != null)
                    {
                        portalFound = true;
                    }
                }
            }

            //add cookie and set Portal on HomeViewModel 
            if (portalFound)
            {
                var precinctCookie = new HttpCookie("PrecinctCode", portal.PrecinctCode);
                precinctCookie.Expires.AddDays(365);
                HttpContext.Response.Cookies.Add(precinctCookie);
            }

            var model = new HomeViewModel
            {
                Portal = portal,
                Posts = svc.GetPosts(null, null, null, 1, 3)
            };
            return View(model);
        }

        /// <summary>
        /// capped == null then show all
        /// capped == true then show capped
        /// capped == false then show uncapped
        /// </summary>
        /// <param name="capped"></param>
        /// <returns></returns>
        public JsonResult GetFilteredProducts(int? locId, int? ispId, bool? capped, bool? isM2MClientContract)
        {
            var ids = svc.GetGriddedProductIds(locId, null, ispId, capped, isM2MClientContract);
            return Json(new { ids }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Interest()
        {
            TempData["IsProdOrder"] = true;
            return RedirectToAction("Register", "Account");
        }

        [HttpPost]
        public ActionResult Interest(bool isProdOrder = false)
        {
            TempData["IsProdOrder"] = isProdOrder;
            return RedirectToAction("Register", "Account");
        }

        public ActionResult BEECertificate()
        {
            return File("~/Content/Frogfoot-BEECertificate.pdf", "application/pdf", "Frogfoot-BEECertificate.pdf");
        }

        [AllowAnonymous]
        public ActionResult GetZoneDataForMaps()
        {
            var model = svc.GetZonesForMaps().Select(x => new {x.ZoneId, x.Code, x.Status, FibreStatusName = x.Status.GetDisplayName(), x.FirstDateOfFibre, x.LastDateOfFibre});
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult GetLocationDataForMaps()
        {
            var model = svc.GetLocations().Select(x => x.APIName).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Redirects from the Home controller are because of the horrible /Home/Home url.
        /// Links have been moved to Frogfoot and Packages controllers respectively 
        /// </summary>
        
        #region Redirected
        //redirected
        public ActionResult Privacy()
        {
            return RedirectToActionPermanent("Privacy", "Frogfoot");
        }

        //redirected
        public ActionResult TermsOfUse()
        {
            return RedirectToActionPermanent("TermsOfUse", "Frogfoot");
        }

        //redirected
        public ActionResult FAQ()
        {
            return RedirectToAction("FAQ", "Frogfoot");
        }

        //redirected
        public ActionResult About()
        {
            //moved action - redirect
            return RedirectToActionPermanent("About", "Frogfoot");
        }

        //redirected
        public ActionResult WhatIsFTTH()
        {
            //moved action - redirect
            return RedirectToActionPermanent("WhatIsFTTH", "Frogfoot");
        }

        //redirected
        public ActionResult Contact()
        {
            //moved action - redirect
            return RedirectToActionPermanent("Contact", "Frogfoot");
        }

        //redirected
        public ActionResult Coverage()
        {
            return RedirectToActionPermanent("Coverage", "Frogfoot");
        }

        //redirected
        public ActionResult Packages()
        {
            return RedirectToActionPermanent("Index", "Packages");
        }

        //redirected
        public ActionResult Product(int prodId, ContractTerm? contractTerm, bool? isTwoOptions)
        {
            return RedirectToActionPermanent("Product", "Packages");
        }
        #endregion
    }
}