using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrogFoot.Models;

namespace FrogFoot.Entities
{
    [Table("FFProduct")]
    public class FFProduct
    {
        public FFProduct()
        {
            FFOrderFFProducts = new HashSet<OrderFFProduct>();
        }

        public int FFProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        public decimal? UnitPriceOnceOff { get; set; }
        public decimal? UnitPriceMonthly { get; set; }
        public decimal? M2MUnitPriceMonthly { get; set; }
        public decimal? M2MUnitPriceOnceOff { get; set; }
        public int? Quantity { get; set; }
        public LineSpeed LineSpeed { get; set; }
        public ProductType Type { get; set; }
        public virtual ICollection<OrderFFProduct> FFOrderFFProducts { get; set; }
    }
}
