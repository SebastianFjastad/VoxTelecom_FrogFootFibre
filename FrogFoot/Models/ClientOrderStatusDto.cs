using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Models
{
    public class ClientOrderStatusDto
    {
        public User Client { get; set; }
        public bool ClientExists { get; set; }
        public bool UserAllowedToOrder { get; set; }
        public bool UserHasOrder { get; set; }

        public Special Special { get; set; }
    }
}