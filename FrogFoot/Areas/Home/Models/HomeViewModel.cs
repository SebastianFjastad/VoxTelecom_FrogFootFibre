using System.Collections.Generic;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Home.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Posts = new List<Post>();
            Portal = new Portal();
        }

        public List<Post> Posts { get; set; }
        public Portal Portal { get; set; }
    }
}