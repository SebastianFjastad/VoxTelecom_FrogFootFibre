using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models.Reports
{
    public class OrderData
    {
        public string Name { get; set; }
        public int CountVox { get; set; }
        public int CountAll { get; set; }
        public decimal? FFValue { get; set; }
        public decimal? VoxValue { get; set; }
        public decimal? OtherValue { get; set; }
        public decimal? GroupValue { get; set; }
        public decimal? VoxPercOfOrders { get; set; }
        public decimal? VoxPercOfRevenue { get; set; }
        public decimal? Penetration { get; set; }
        public int Residents { get; set; }
        public decimal? FFProdOrderPerc { get; set; }
        public decimal? VoxProdOrderPerc { get; set; }
        public LineSpeed? LineSpeed { get; set; }
        public bool IsM2MFrogfootLink { get; set; }
        public decimal? FFARPU { get; set; }   
        public decimal? GroupARPU { get; set; }   
        public decimal? AverageCappedPrice { get; set; }
        public decimal? AverageUncappedPrice { get; set; }
        public decimal? AveragePrice { get; set; }

        #region All Order properties
        public DateTime CreatedDate { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool UserHasOrder { get; set; }  
        public int? LocationId { get; set; }
        public DateTime MonthPeriod { get; set; }
        public decimal? FFMonthlyRevenue { get; set; }
        public decimal? FFSetupRevenue { get; set; }
        public decimal? ISPMonthlyRevenue { get; set; }
        public decimal? ISPSetupRevenue { get; set; }
        public int? ISPId { get; set; }
        #endregion

        
    }
}