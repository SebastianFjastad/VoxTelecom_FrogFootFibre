using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FrogFoot.Entities
{
    public class Asset
    {
        public int AssetId { get; set; }
        public string AssetPath { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ISPProductId { get; set; }
        public int? OrderId { get; set; }
    }
}