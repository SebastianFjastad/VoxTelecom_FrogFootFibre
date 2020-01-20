using System.Collections.Generic;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Client.Models
{
    public class OrdersViewModel
    {
        public OrdersViewModel()
        {
            Orders = new List<Order>();
            User = new User();
            
        }

        public User User { get; set; }

        public List<Order> Orders { get; set; }
    }
}