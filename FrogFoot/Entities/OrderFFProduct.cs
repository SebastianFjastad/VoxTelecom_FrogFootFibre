using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FrogFoot.Entities
{
    public class OrderFFProduct
    {
        public int OrderFFProductId { get; set; }

        public int FFProductId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public Order Order { get; set; }
        public FFProduct FFProduct { get; set; }
    }
}