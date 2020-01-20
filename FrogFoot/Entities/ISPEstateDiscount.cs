using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class ISPEstateDiscount
    {
        public int ISPEstateDiscountId { get; set; }

        public int ISPId { get; set; }
        public ISP ISP { get; set; }

        public int EstateId { get; set; }
        public Estate Estate { get; set; }

        public Discount Discount { get; set; }
    }
}