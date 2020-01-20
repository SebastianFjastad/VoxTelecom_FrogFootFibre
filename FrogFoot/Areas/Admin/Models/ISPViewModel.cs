using System.Collections.Generic;
using FrogFoot.Models;
using FrogFoot.Entities;

namespace FrogFoot.Areas.Admin.Models
{
    public class ISPViewModel : ViewModelBase
    {
        public ISPViewModel()
        {
            ISP = new ISP();
            ISPs = new List<ISP>();
        }

        public ISP ISP { get; set; }

        public List<ISP> ISPs { get; set; } 
    }
}