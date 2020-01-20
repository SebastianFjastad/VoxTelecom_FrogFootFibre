using System.Collections.Generic;
using FrogFoot.Entities;

namespace FrogFoot.Models
{
    public class UserDto
    {
        public UserDto()
        {
            OrdersObj = new List<Order>();
            ClientContactMethods = new List<ClientContactMethod>();
        }
        /// <summary>
        /// The properties in this DTO are used across different server-side datatables responses.
        /// IMPORTANT: The names of the properties are also used client side (.cshtml) for rendering table columns
        /// so if changed then the string names need to be changed in the javascript
        /// </summary>

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Landline { get; set; }
        public string Address { get; set; }
        public string Precinct { get; set; }
        public string Zone { get; set; }
        public string ZoneStatus { get; set; }
        public string Role { get; set; }
        public string ChampCoverage { get; set; }
        public string ISPName { get; set; }
        public string Ordered { get; set; }
        public bool UsePrecinctCodeForChamp { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowCell { get; set; }
        public bool ShowLandline { get; set; }
        public string IsSMS { get; set; }
        public bool IsClientContacted { get; set; }
        public Location LocationObj { get; set; }
        public Zone ZoneObj { get; set; }
        public Estate EstateObj { get; set; }
        public ISP ISPObj { get; set; }
        public ISPClientContact ISPClientContactObj { get; set; }
        public ICollection<Order> OrdersObj { get; set; }
        public ICollection<ClientContactMethod> ClientContactMethods { get; set; } 
    }
}