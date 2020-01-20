using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using FrogFoot.Models;
using Microsoft.SqlServer.Server;

namespace FrogFoot.Entities
{
    public class ISPProduct
    {
        public ISPProduct()
        {
            Orders = new HashSet<Order>();
        }

        public int ISPProductId { get; set; }

        [Required]
        [Display(Name = "Line speed")]
        public LineSpeed LineSpeed { get; set; }

        [Required]
        [Display(Name = "Up speed")]
        public string UpSpeed { get; set; }

        [Display(Name = "Capped")]
        public bool IsCapped { get; set; }

        public int? Cap { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsActive { get; set; }

        public string ProductName { get; set; }

        [Required]
        public string Description { get; set; }

        [Display(Name = "Router")]
        public string Attr1 { get; set; }

        [Display(Name = "VoIP/Phone")]
        public string Attr2 { get; set; }

        [Display(Name = "Capping")]
        public string Attr3 { get; set; }

        public string Attr4 { get; set; }

        public string Info1Heading { get; set; }
        public string Info1 { get; set; }

        public string Info2Heading { get; set; }
        public string Info2 { get; set; }

        public string Info3Heading { get; set; }
        public string Info3 { get; set; }

        public string Info4Heading { get; set; }
        public string Info4 { get; set; }

        [Display(Name = "24M Monthly cost")]
        public decimal? MonthlyCost { get; set; }

        [Display(Name = "M2M Once Off FF Payment")]
        public bool OnceOfFFPaymentForM2M { get; set; }

        [Display(Name = "24M Setup cost")]
        public decimal? SetupCost { get; set; }

        [Display(Name = "M2M Setup cost")]
        public decimal? M2MSetupCost { get; set; }

        [Display(Name = "M2M Monthly cost")]
        public decimal? M2MMonthlyCost { get; set; }

        [Display(Name = "M2M Client Contract")]
        public bool IsM2MClient { get; set; }

        [Display(Name = "24M Client Contract")]
        public bool Is24MClient { get; set; }

        [Display(Name = "Frogfoot Line Options")]
        public bool IsM2MFrogfootLink { get; set; }

        public bool? Shaped { get; set; }

        [Display(Name = "Router Incl.")]
        public bool? Router { get; set; }

        [Display(Name = "Phone Incl.")]
        public bool? Phone { get; set; }

        [Display(Name = "Free Installation")]
        public bool? Install { get; set; }

        [Display(Name = "Video streaming")]
        public bool? Video { get; set; }

        [Display(Name = "Mobile Data  Incl.")]
        public bool? MobileData { get; set; }

        public bool? CCTV { get; set; }

        public int? ISPId { get; set; }
        public virtual ISP ISP { get; set; }

        public bool? IsSpecial { get; set; }

        public virtual Asset ISPLogo { get; set; }

        public virtual ICollection<ISPLocationProduct> ISPLocationProducts { get; set; } 

        public virtual ICollection<ISPEstateProduct> ISPEstateProducts { get; set; } 

        public ICollection<Order> Orders { get; set; }
    }
}
