using System;
using FrogFoot.Areas.Home.Models;

namespace FrogFoot.Entities
{
    public class ISPClientContact
    {
        public int ISPClientContactId { get; set; }

        public DateTime ContactCreatedDate { get; set; }
        public DateTime? ContactMadeDate { get; set; }
        public bool IsContacted { get; set; }

        public string ClientId { get; set; }
        public User Client { get; set; }

        public int ISPId { get; set; }

        public string ISPUserId { get; set; }
    }
}