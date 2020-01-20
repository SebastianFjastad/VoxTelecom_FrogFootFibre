using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Models.Reports;

namespace FrogFoot.Areas.Admin.Models
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
            Reports = new List<ReportDataDto>();
            Locations = new List<Location>();
            OrdersByLocation = new List<OrderData>();
            OrdersByFFProduct = new List<OrderData>();
            OrdersByMonth = new List<OrderData>();
            AllOrders = new List<OrderData>();
            MonthlyOverview = new List<MonthlyOverview>();
            ARPU = new List<ARPU>();
        }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? LocationId { get; set; }
        public List<Location> Locations { get; set; }
        public List<ReportDataDto> Reports { get; set; }

        public List<OrderData> OrdersByLocation { get; set; }
        public List<OrderData> OrdersByFFProduct { get; set; }
        public List<OrderData> OrdersByMonth { get; set; }
        public List<ISPReportData> OrdersByISP { get; set; }
        public List<OrderData> AllOrders { get; set; }
        public List<UserInterestData> UsersWithOrders { get; set; }
        public List<MonthlyOverview> MonthlyOverview { get; set; }
        public List<ARPU> ARPU { get; set; }

        #region Common SUM properties
        public int SumVoxOrderCount { get; set; }
        public int SumAllOrderCount { get; set; }
        public decimal? SumFrogfootValue { get; set; }
        public decimal? SumVoxValue { get; set; }
        public decimal? SumOtherValue { get; set; }
        public decimal? SumGroupValue { get; set; }
        public decimal? AvgVoxPercOrders { get; set; }
        public decimal? AvgVoxPercRevenue { get; set; }
        public decimal? AvgFFARPU { get; set; }
        public decimal? AvgGroupARPU { get; set; }
        #endregion

        #region unique sum properties
        public decimal? AvgPercPenetration { get; set; }
        public int SumResidents { get; set; }
        public decimal? AvgPercTotalOrders { get; set; }
        public decimal? AvgVoxPercTotalOrders { get; set; }
        public decimal? AvgCappedPrice { get; set; }
        public decimal? AvgUncappedPrice { get; set; }
        public decimal? AvgPrice { get; set; }
        #endregion
    }
}