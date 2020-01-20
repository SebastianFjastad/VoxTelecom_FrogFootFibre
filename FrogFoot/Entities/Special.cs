using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class Special
    {
        public int SpecialId { get; set; }
        public int? TimePeriodMonths { get; set; }
        public LineSpeed SpecialLineSpeed { get; set; }
        public Discount Discount { get; set; }
        public SpecialType SpecialType { get; set; }
        public bool IsDeleted { get; set; }
    }
}