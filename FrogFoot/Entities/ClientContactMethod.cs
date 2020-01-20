using FrogFoot.Areas.Home.Models;

namespace FrogFoot.Entities
{
    public class ClientContactMethod
    {
        public int ClientContactMethodId { get; set; }

        public bool IsSelected { get; set; }

        public int ContactMethodId { get; set; }
        public ContactMethod ContactMethod { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}