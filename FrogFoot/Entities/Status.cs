using System;
using FrogFoot.Models;

namespace FrogFoot.Entities
{
    public class Status
    {
        public int StatusId { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}