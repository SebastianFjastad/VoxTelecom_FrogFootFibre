using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;

namespace FrogFoot.Utilities
{
    public static class FindPortalLists
    {
        public static List<Portal> Portals { get; set; } 
        public static List<Url> Urls { get; set; } 
        public static List<Location> Locations { get; set; } 
    }
}