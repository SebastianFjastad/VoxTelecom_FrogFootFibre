using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;
using FrogFoot.Services;

namespace FrogFoot.Areas.FFManager.Models
{
    public class GriddingViewModel
    {
        public GriddingViewModel()
        {
            Locations = new List<Location>();
            Estates = new List<Estate>();
            Location = new Location();
            Estate = new Estate();
        }

        public Location Location { get; set; }
        public List<Location> Locations { get; set; }
        public Estate Estate { get; set; }
        public List<Estate> Estates { get; set; }
    }
}