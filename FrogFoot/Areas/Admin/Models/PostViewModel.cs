using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Entities;
using FrogFoot.Models;
using Hangfire.Annotations;

namespace FrogFoot.Areas.Admin.Models
{
    public class PostViewModel
    {
        public PostViewModel()
        {
            Post = new Post();
            Locations = new List<Location>();
            Zones = new List<Zone>();
            Precincts = new List<PrecinctDto>();
        }

        public Post Post { get; set; }
        public List<Location> Locations { get; set; }
        public List<PrecinctDto> Precincts { get; set; }
        public List<Zone> Zones { get; set; }
    }
}