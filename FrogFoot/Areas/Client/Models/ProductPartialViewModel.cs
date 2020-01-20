using System;
using System.Collections.Generic;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Client.Models
{
    public class ProductPartialViewModel
    {
        public ProductPartialViewModel()
        {
            Products = new List<ISPProduct>();
            UserZone = new Zone();
            Specials = new List<Special>();
            User = new User();
        }

        public List<ISPProduct> Products { get; set; }
        public Zone UserZone { get; set; }
        public List<Special> Specials { get; set; }
        public User User { get; set; }
    }
}