using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Areas.Admin.Models
{
    public class FFProdEditDto
    {
        public int id{ get; set; }
        public int qty { get; set; }
        public bool? action { get; set; }
    }
}