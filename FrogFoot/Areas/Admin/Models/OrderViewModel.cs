using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.Admin.Models
{
    public class OrderViewModel : ViewModelBase, IOrderViewModel
    {
        public OrderViewModel()
        {
            Order = new Order();
            Orders = new List<Order>();
            Locations = new List<Location>();
            Estates = new List<Estate>();
            Products = new List<FFProduct>();
        }

        public Order Order { get; set; }
        public List<Order> Orders { get; set; }
        public List<ISPProduct> ISPProducts { get; set; }
        public List<Location> Locations { get; set; }
        public List<Estate> Estates { get; set; }
        public List<FFProduct> Products { get; set; }

        public List<int> QuantityId { get; set; }
        public List<int> QuantityQty { get; set; }

        public List<int> OptionId { get; set; }
        public List<bool> Option { get; set; }

    }
}