using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models
{
    public class PrecinctDto
    {
        public string Name { get; set; }
        public string PrecinctCode { get; set; }
        public int LocationId { get; set; }
    }
}