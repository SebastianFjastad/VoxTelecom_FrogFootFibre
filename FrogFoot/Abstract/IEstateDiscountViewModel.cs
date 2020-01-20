using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogFoot.Entities;
using FrogFoot.Models;

namespace FrogFoot.Abstract
{
    public interface IEstateDiscountViewModel
    {
         Estate Estate { get; set; }

         List<ISP> ISPs { get; set; }

         Discount Discount { get; set; }
    }
}
