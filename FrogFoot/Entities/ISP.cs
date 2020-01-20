using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Microsoft.SqlServer.Server;

namespace FrogFoot.Entities
{
    [Table("ISP")]
    public class ISP
    {
        public ISP()
        {
            Orders = new HashSet<Order>();
            ISPProducts = new HashSet<ISPProduct>();
            LocationProducts = new HashSet<ISPLocationProduct>();
            EstateProducts = new HashSet<ISPEstateProduct>();
        }

        public int? ISPId { get; set; }

        public bool IsDeleted { get; set; }

        public bool AllowViewLeads { get; set; }

        public bool ImmediateOrderEmailNotification { get; set; }

        [Required]
        [Display(Name="ISP Code")]
        public string ISPCode { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string EmailAddress { get; set; }

        public string CellNo { get; set; }

        [Required]
        [StringLength(20)]
        public string LandlineNo { get; set; }

        [AllowHtml]
        public string TermsAndConditions { get; set; }

        public virtual ICollection<Order> Orders { get; set; }

        public virtual ICollection<ISPEstateDiscount> ISPEstateDiscounts { get; set; } 

        public virtual ICollection<ISPProduct> ISPProducts { get; set; }

        public virtual ICollection<ISPLocationProduct> LocationProducts { get; set; }

        public virtual ICollection<ISPEstateProduct> EstateProducts { get; set; }
    }
}
