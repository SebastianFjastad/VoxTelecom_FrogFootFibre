using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class Log
    {
        public int LogId { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Message { get; set; }
        public DateTime? TimeStamp { get; set; }
        public UserAction Type { get; set; }
        public OrderStatus? OrderStatus { get; set; }
    }
}