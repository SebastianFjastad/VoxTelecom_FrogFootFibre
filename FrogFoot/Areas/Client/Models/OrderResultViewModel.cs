using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Client.Models
{
    public class OrderResultViewModel
    {
        public OrderResultViewModel()
        {
            User = new User();
            ISP = new ISP();
        }

        public bool OrderResult { get; set; }

        public User User { get; set; }

        public ISP ISP { get; set; }
    }
}