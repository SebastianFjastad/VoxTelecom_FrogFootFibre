using System.Collections.Generic;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Admin.Models
{
    public class UserViewModel : ViewModelBase
    {
        public UserViewModel()
        {
            ISPList = new List<ISP>();
            User = new User();
            Locations = new List<Location>();
            Estates = new List<Estate>();
        }

        public UserType Type { get; set; }

        public IEnumerable<ISP> ISPList { get; set; }

        public string UserPassword { get; set; }

        public User User { get; set; }

        public List<User> Users { get; set; } 

        public List<Location> Locations { get; set; } 
        public List<Estate> Estates { get; set; } 
    }
}