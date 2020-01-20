using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.ISPAdmin.Models
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
            Reports = new List<ReportDataDto>();
            Locations = new List<Location>();
        }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int? LocationId { get; set; }

        public List<Location> Locations { get; set; }
        public List<ReportDataDto> Reports { get; set; }
    }
}