using System.Collections.Generic;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.Admin.Models
{
    public class ZoneViewModel
    {
        public ZoneViewModel()
        {
            Zone = new Zone();
            Zones = new List<Zone>();
            Precincts = new List<PrecinctDto>();
        }

        public Zone Zone { get; set; }
        public List<Zone> Zones { get; set; }
        public List<PrecinctDto> Precincts { get; set; }  

    }
}