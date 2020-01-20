using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models.Reports
{
    public class UserInterestData
    {
        public DateTime? CreatedDate { get; set; }
        public DateTime? RegisteredDate { get; set; }
        public int? LocationId { get; set; }
        public int? ZoneId { get; set; }
        public bool? UserHasOrder { get; set; }
        public string PrecinctCode { get; set; }
    }
}