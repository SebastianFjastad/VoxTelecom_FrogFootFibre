using System.Collections.Generic;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.ISPAdmin.Models
{
    public class OrderViewModel : ViewModelBase
    {
        public OrderViewModel()
        {
            Order = new Order();
            Orders = new List<Order>();
            Products = new List<FFProduct>();
            Locations = new List<Location>();
            Estates = new List<Estate>();
            ISPProducts = new List<ISPProduct>();
        }

        public Order Order { get; set; }
        public List<Order> Orders { get; set; }
        public List<FFProduct> Products { get; set; }
        public List<ISPProduct> ISPProducts { get; set; }
        public List<Location> Locations { get; set; }
        public List<Estate> Estates { get; set; }
        public List<ISPEstateDiscount> Discounts { get; set; }

        public List<int> QuantityId { get; set; } 
        public List<int> QuantityQty { get; set; }
        
        public List<int> OptionId { get; set; }
        public List<bool> Option { get; set; }
 

    }
}