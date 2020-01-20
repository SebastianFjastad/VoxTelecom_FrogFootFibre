using System.Collections.Generic;
using FrogFoot.Abstract;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Areas.FFManager.Models
{
    public class EstateDiscountViewModel : IEstateDiscountViewModel
    {
        public EstateDiscountViewModel()
        {
            Estate = new Estate();
            ISPs = new List<ISP>();
        }

        public Estate Estate { get; set; }

        public List<ISP> ISPs { get; set; }

        public Discount Discount { get; set; }
    }
}