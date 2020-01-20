
using FrogFoot.Areas.Home.Models;

namespace FrogFoot.Entities
{
    public class ClientISPContact
    {
        public int ClientISPContactId { get; set; }

        public bool IsISPSelected { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int ISPId { get; set; }
        public ISP ISP { get; set; }
    }
}