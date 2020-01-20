using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Admin.Models
{
    public class ISPProductViewModel
    {
        public ISPProductViewModel()
        {
            ISP = new ISP();
            ISPs = new List<ISP>();
            ISPProduct = new ISPProduct();
            ISPProducts = new List<ISPProduct>();
        }

        public int ISPId { get; set; }
        public string ISPName { get; set; }

        public ISP ISP { get; set; }

        public List<ISP> ISPs { get; set; }

        public ISPProduct ISPProduct { get; set; }

        public List<ISPProduct> ISPProducts { get; set; } 
    }
}