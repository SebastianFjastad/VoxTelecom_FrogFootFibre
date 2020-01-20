using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FrogFoot.Entities
{
    public class Estate
    {
        public Estate()
        {
            ISPEstateDiscounts = new HashSet<ISPEstateDiscount>();
        }

        public int EstateId { get; set; }

        public string  EstateCode { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public int LocationId { get; set; }

        public virtual Location Location { get; set; }
        public virtual ICollection<ISPEstateDiscount> ISPEstateDiscounts { get; set; }
        public virtual ICollection<ISPEstateProduct> ISPEstateProducts { get; set; }
    }
}