using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Management;
using FrogFoot.Areas.Home.Models;

namespace FrogFoot.Models
{
    public class UserSyncResponseDto
    {
        public bool ZoneAssigned { get; set; }
        public bool IsPossible { get; set; }
        public User User { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
    }
}