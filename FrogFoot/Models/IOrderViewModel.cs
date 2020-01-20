using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogFoot.Entities;

namespace FrogFoot.Models
{
    interface IOrderViewModel
    {
        Order Order { get; set; }
        List<Order> Orders { get; set; } 
    }
}
