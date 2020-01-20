using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls.WebParts;
using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class Post
    {
        public int PostId { get; set; }

        #region Post Gridding
        public int? LocationId { get; set; }
        public virtual Location Location { get; set; }
        public string PrecinctCode { get; set; }
        public int? ZoneId { get; set; }
        public virtual Zone Zone { get; set; }
        public PostGridding GridType { get; set; }
        public string CreatedByRole { get; set; }
        public bool IsEmail { get; set; }
        #endregion

        #region Post Details
        [Required]
        public string Title { get; set; }
        [Required]
        [AllowHtml]
        public string Body { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime PublishDate { get; set; }
        public bool IsDeleted { get; set; }
        public PostType Type { get; set; }
        public virtual Asset PostImage { get; set; }
        #endregion
    }
}