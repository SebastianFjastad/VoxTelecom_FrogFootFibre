using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models.Reports
{
    public class ARPU
    {
        public LineSpeed FFProduct { get; set; }
        public decimal? Frogfoot { get; set; }
        public decimal? Vox { get; set; }
        public decimal? Group { get; set; }
    }
}