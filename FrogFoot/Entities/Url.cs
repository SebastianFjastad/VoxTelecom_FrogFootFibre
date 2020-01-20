using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Entities
{
    public class Url
    {
        public int UrlId { get; set; }
        public string URL { get; set; }
        public int PortalId { get; set; }
    }
}