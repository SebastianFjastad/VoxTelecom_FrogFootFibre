using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FrogFoot.Entities
{
    public class Portal
    {
        public int PortalId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DisplayName("Precinct Code")]
        public string PrecinctCode { get; set; }
        [DisplayName("Facebook URL")]
        public string FacebookUrl { get; set; }
        [DisplayName("Twitter URL")]
        public string TwitterUrl { get; set; }
        public bool IsDeleted { get; set; }
        public Asset CoverImage { get; set; }
        public virtual List<Url> Urls { get; set; } 
    }
}