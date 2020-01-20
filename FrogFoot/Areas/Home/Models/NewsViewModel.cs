using System.Collections.Generic;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Home.Models
{
    public class NewsViewModel
    {
        public NewsViewModel()
        {
            Post = new Post();
            Posts = new List<Post>();
        }

        public List<Post> Posts { get; set; }
        public Post Post { get; set; }
    }
}