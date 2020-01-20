using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace FrogFoot.Models
{
    public class EmailDto
    {
        public string Subject { get; set; }
        [JsonProperty(PropertyName = "Html-part")]
        public string Body { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}