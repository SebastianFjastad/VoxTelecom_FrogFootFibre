using System.Collections.Generic;
using FrogFoot.Entities;

namespace FrogFoot.Areas.ISPAdmin.Models
{
    public class ISPProductViewModel
    {
        public ISPProductViewModel()
        {
            ISPProduct = new ISPProduct();
            ISPProducts = new List<ISPProduct>();
        }

        public ISPProduct ISPProduct { get; set; }

        public List<ISPProduct> ISPProducts { get; set; }

    }
}