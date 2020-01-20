using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FrogFoot.Models
{
    public class ViewModelBase
    {
        public bool HasErrors { get; set; }

        public string Message { get; set; }
    }
}