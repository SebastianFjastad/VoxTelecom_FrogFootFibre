using System;
using System.ComponentModel.DataAnnotations;
using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class Zone
    {
        public int ZoneId { get; set; }
        [Required]
        public string Code { get; set; }

        public TrenchingStatus Status { get; set; }
        public bool AllowOrder { get; set; }
        public bool AllowSpecial { get; set; }
        public bool IsDeleted { get; set; }
        public bool AllowLeads { get; set; }
        [Display(Name = "First Date Of Fibre")]
        public DateTime? FirstDateOfFibre { get; set; }
        [Display(Name = "Last Date Of Fibre")]
        public DateTime? LastDateOfFibre { get; set; }
        public string TimeToInstallation { get; set; }
        [Required]
        [Display(Name = "Precinct Code")]
        public string PrecinctCode { get; set; }
        [Display(Name = "No. Houses In Zone")]
        public int NoHousesInZone { get; set; }

        [Display(Name = "Node Id")]
        public string NodeId { get; set; }
        [Display(Name = "Node Name")]
        public string NodeName { get; set; }
        [Display(Name = "Node Latitude")]
        public double? NodeLatitude { get; set; }
        [Display(Name = "Node Longitude")]
        public double? NodeLongitude { get; set; }
    }
}