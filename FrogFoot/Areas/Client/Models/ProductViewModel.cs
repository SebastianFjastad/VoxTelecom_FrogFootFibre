using System.Collections.Generic;
using System.Linq;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Client.Models
{
    public class ProductViewModel
    {
        private List<ISP> _isPs;

        public ProductViewModel()
        {
            Product = new ISPProduct();
            Products = new List<ISPProduct>();
            ISPs = new List<ISP>();
            User = new User();
            Orders = new List<Order>();
            Specials = new List<Special>();
        }

        public bool UserCanOrder { get; set; }
        public ISPProduct Product { get; set; }
        public List<ISPProduct> Products { get; set; }
        public User User { get; set; }
        public List<Order> Orders { get; set; }
        public List<ISP> ISPs
        {
            get { return _isPs.OrderBy(i => i.Name).ToList(); }
            set { _isPs = value; }
        }
        public List<Special> Specials { get; set; }
    }
}