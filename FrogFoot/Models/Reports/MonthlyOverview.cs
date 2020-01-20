using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models.Reports
{
    public class MonthlyOverview
    {
        public string Month { get; set; }
        public decimal? Penetration { get; set; }
        public int Orders { get; set; }
        public decimal? FFRevenue { get; set; }
        public decimal? VoxRevenue { get; set; }
        public decimal? GroupRevenue { get; set; }
        public decimal? FFARPU { get; set; }
        public decimal? GroupARPU { get; set; }
    }
}