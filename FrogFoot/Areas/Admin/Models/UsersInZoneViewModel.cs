using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;
using FrogFoot.Models;
using OfficeOpenXml;

namespace FrogFoot.Areas.Admin.Models
{
    public class UsersInZoneViewModel
    {
        public UsersInZoneViewModel()
        {
            Zones = new List<Zone>();
            Precincts = new List<PrecinctDto>();
        }

        public List<Zone> Zones { get; set; }
        public List<PrecinctDto> Precincts { get; set; }
    }
}