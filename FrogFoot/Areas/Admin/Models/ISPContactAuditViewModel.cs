using System.Collections.Generic;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Admin.Models
{
    public class ISPContactAuditViewModel
    {
        public ISPContactAuditViewModel()
        {
            ISPUsers = new List<User>();
            ISPClientContacts = new List<ISPClientContact>();
        }

        public User Client { get; set; }

        public List<User> ISPUsers { get; set; }

        public List<ISPClientContact> ISPClientContacts { get; set; }
    }
}