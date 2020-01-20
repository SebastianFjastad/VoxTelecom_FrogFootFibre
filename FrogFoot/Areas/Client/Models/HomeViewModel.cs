using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Client.Models
{
    public class HomeViewModel
    {
        public HomeViewModel()
        {
            Posts = new List<Post>();
            User = new User();
            Post = new Post();
           
        }

        public Post Post { get; set; }

        public List<Post> Posts { get; set; }

        public User User { get; set; }
    }
}