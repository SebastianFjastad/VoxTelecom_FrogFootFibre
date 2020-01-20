using System.Collections.Generic;
using System.Linq;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Home.Models
{
    public class PackageViewModel
    {
        private List<ISP> _isPs;

        //private Random rnd = new Random();

        public PackageViewModel()
        {
            Product = new ISPProduct();
            Products = new List<ISPProduct>();
        }

        public ISPProduct Product { get; set; } 
        public List<ISPProduct> Products { get; set; }
        public List<Location> Suburbs { get; set; }

        //get all the ISPs that offer products FROM Products list
        public List<ISP> ISPs
        {
            get
            {
                return Products.GroupBy(o => o.ISPId)
                    .Select(grp => grp.ToList().First().ISP).OrderBy(i => i.Name)
                    .ToList();
            }
            set { _isPs = value; }
        }
    }
}