using System.Collections.Generic;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;

namespace FrogFoot.Areas.Admin.Models
{
    public class FFUsersViewModel : ViewModelBase
    {
        public FFUsersViewModel()
        {
            FFUser = new User();
            FFUsers = new List<User>();
        }

        public User FFUser { get; set; }

        public List<User> FFUsers { get; set; }
    }
}