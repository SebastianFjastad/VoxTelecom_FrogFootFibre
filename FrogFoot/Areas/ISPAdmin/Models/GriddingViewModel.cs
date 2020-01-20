using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;

namespace FrogFoot.Areas.ISPAdmin.Models
{
    public class GriddingViewModel
    {
        public GriddingViewModel()
        {
            ISPs = new List<ISP>();
            Locations = new List<Location>();
            Estates = new List<Estate>();
            ISPLocationProducts = new List<ISPLocationProduct>();
            ISPEstateProducts = new List<ISPEstateProduct>();
        }

        public int ISPId { get; set; }
        public int? EstateId { get; set; }

        public List<ISP> ISPs { get; set; }
        public List<ISPProduct> ISPProducts { get; set; }
        public List<Location> Locations { get; set; }
        public List<Estate> Estates { get; set; }

        public List<ISPLocationProduct> ISPLocationProducts { get; set; }
        public List<ISPEstateProduct> ISPEstateProducts { get; set; }
    }
}