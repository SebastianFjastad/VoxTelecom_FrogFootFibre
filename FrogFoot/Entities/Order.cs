using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class Order
    {
        public Order()
        {
            Logs = new HashSet<Log>();
            OrderFFProducts = new HashSet<OrderFFProduct>();
            PDFs = new HashSet<Asset>();
            StatusList = new HashSet<Status>();
        }

        [Display(Name = "Order Id")]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Line speed")]
        public int FFProductId { get; set; }

        public OrderStatus Status { get; set; }

        [Display(Name = "Frogfoot Order no.")]
        public string FFOrderNo { get; set; }

        [StringLength(50)]
        [Display(Name = "ISP Order no.")]
        public string ISPOrderNo { get; set; }

        [Column(TypeName = "date")]
        [Display(Name = "Created date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name="Created by")]
        public string CreatedByRole { get; set; }

        public int? ISPProductId { get; set; }
        public ISPProduct ISPProduct { get; set; }

        public ContractTerm ClientContractTerm { get; set; }

        public bool? IsSpecial { get; set; }

        public Special Special { get; set; }

        public string ClientId { get; set; }
        public User Client { get; set; }

        public int? ISPId { get; set; }
        public virtual ISP ISP { get; set; }

        public int LocationId { get; set; }
        public virtual Location Location { get; set; }

        public virtual ICollection<Status> StatusList { get; set; }

        public virtual ICollection<Asset> PDFs { get; set; }

        public virtual ICollection<Log> Logs { get; set; } 

        public virtual ICollection<OrderFFProduct> OrderFFProducts { get; set; }
    }
}
