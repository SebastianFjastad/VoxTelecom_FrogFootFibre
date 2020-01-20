using System.Collections.Generic;
using System.Linq;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Client.Models
{
    public class ProfileViewModel
    {
        private List<ISP> _isPs;

        public ProfileViewModel()
        {
            User = new User();
            IndexViewModel = new IndexViewModel();
            Locations = new List<Location>();
            Estates = new List<Estate>();
            ClientISPs = new List<ClientISPContact>();
            ContactMethods = new List<ContactMethod>();
            ClientContactMethods = new List<ClientContactMethod>();
            ISPs = new List<ISP>();
        }

        public List<ContactMethod> ContactMethods { get; set; }

        public List<ClientContactMethod> ClientContactMethods { get; set; } 

        public User User { get; set; }

        public List<Location> Locations { get; set; }

        public List<Estate> Estates { get; set; }

        public List<ISP> ISPs
        {
            get { return _isPs.OrderBy(i => i.Name).ToList(); }
            set { _isPs = value; }
        }

        public List<ClientISPContact> ClientISPs { get; set; }

        public IndexViewModel IndexViewModel { get; set; }
    }
}