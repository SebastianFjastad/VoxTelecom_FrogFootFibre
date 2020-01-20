using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FrogFoot.Entities
{
    public class ISPLocationProduct
    {
        public int ISPLocationProductId { get; set; }

        public int ISPProductId { get; set; }
        public ISPProduct ISPProduct { get; set; }

        public int ISPId { get; set; }
        public ISP ISP { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }
    }
}