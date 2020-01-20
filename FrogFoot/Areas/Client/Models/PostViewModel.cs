using System;
using System.Collections.Generic;
using System.Linq;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using OfficeOpenXml;

namespace FrogFoot.Areas.Client.Models
{
    public class PostViewModel
    {
        public PostViewModel()
        {
            Post = new Post();
            Posts = new List<Post>();
            User = new User();
            Zones = new List<Zone>();
        }

        public List<Post> Posts { get; set; }
        public Post Post { get; set; }
        public User User { get; set; }
        public List<Zone> Zones { get; set; }
        public List<Location> Locations { get; set; }
    }
}