using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models.Reports
{
    public class ISPReportData
    {
        public string ISPName { get; set; }
        public int Orders { get; set; }
        public decimal BBValue { get; set; }
        public decimal LinksValue { get; set; }
        public decimal ISPPercOfOrders { get; set; }
        public decimal ISPPercOfRevenue { get; set; }
        public decimal Penetration { get; set; }
        public int Residents { get; set; }
        public int StatusNew { get; set; }
        public int StatusPending { get; set; }
        public int StatusOrdered { get; set; }
        public int StatusAccepted { get; set; }
    }
}