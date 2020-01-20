using System.Collections.Generic;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Admin.Models
{
    public class PortalViewModel
    {
        public PortalViewModel()
        {
            Portals = new List<Portal>();
        }

        public Portal Portal { get; set; }

        public List<Portal> Portals { get; set; }
    }
}