using System.Collections.Generic;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.Admin.Models
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            Location = new Location();
            Locations = new List<Location>();
            Estate = new Estate();
            Estates = new List<Estate>();
        }

        public Location Location { get; set; }
        public List<Location> Locations { get; set; }

        public Estate Estate { get; set; }
        public List<Estate> Estates { get; set; }
    }
}