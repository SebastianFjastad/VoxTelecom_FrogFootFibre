using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Areas.ISPAdmin.Models
{
    public class ProductActiveDto
    {
        public int prodId { get; set; }
        public bool isActive { get; set; }
    }
}