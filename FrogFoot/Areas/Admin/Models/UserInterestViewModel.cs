using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models.Reports;

namespace FrogFoot.Areas.Admin.Models
{
    public class UserInterestViewModel
    {
        public UserInterestViewModel()
        {
            Users = new List<User>();
            Locations = new List<Location>();
            Zones = new List<Zone>();
        }

        public List<UserInterestData> UsersAndOrders { get; set; }
        public List<User> Users { get; set; } 
        public List<Location> Locations { get; set; } 
        public List<Zone> Zones { get; set; } 
    }
}