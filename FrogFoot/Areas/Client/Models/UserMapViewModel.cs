using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;

namespace FrogFoot.Areas.Client.Models
{
    public class UserMapViewModel
    {
        public UserMapViewModel()
        {
            User = new User();
            Users = new List<User>();
        }

        public User User { get; set; }
        public List<User> Users { get; set; }
    }
}