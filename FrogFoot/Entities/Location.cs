using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrogFoot.Entities
{
    [Table("Location")]
    public class Location
    {
        public Location()
        {
            FFOrders = new HashSet<Order>();
            Estates = new HashSet<Estate>();
            ISPLocationProducts = new HashSet<ISPLocationProduct>();
        }

        public int LocationId { get; set; }
        [Display(Name = "Code")]
        public string PrecinctCode { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Location")]
        public string Name { get; set; }
        public string APIName { get; set; }

        public bool IsActive { get; set; }

        public bool AllowOrder { get; set; }

        public bool IsDeleted { get; set; }

        public int Residents { get; set; }

        public virtual ICollection<Order> FFOrders { get; set; }

        public virtual ICollection<Estate> Estates { get; set; } 

        public virtual ICollection<ISPLocationProduct> ISPLocationProducts { get; set; } 
    }
}
