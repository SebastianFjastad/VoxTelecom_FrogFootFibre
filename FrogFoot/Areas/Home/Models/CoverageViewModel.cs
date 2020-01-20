using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Home.Models
{
    public class CoverageViewModel
    {
        public CoverageViewModel()
        {
            Locations = new List<Location>();
            Estates = new List<Estate>();
        }

        public List<Location> Locations { get; set; }
        public List<Estate> Estates { get; set; }

    }
}