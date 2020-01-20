using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using Microsoft.Owin.Security;

namespace FrogFoot.Areas.Client.Models
{
    public class ContactViewModel
    {
        public ContactViewModel()
        {
            User = new User();
        }

        [Required]
        public string Subject { get; set; }
        [Required]
        public string Message { get; set; }

        public User User { get; set; }
    }
}