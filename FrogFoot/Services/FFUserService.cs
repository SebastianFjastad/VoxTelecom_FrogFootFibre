using System.Collections.Generic;
using System.Linq;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Repositories;

namespace FrogFoot.Services
{
    public class FFUserService
    {
        private OrderRepository repo = new OrderRepository();

        public List<Order> GetOrders()
        {
            return repo.GetOrders().ToList();
        }

        public Order GetOrder(int id)
        {
            return repo.GetOrder(id);
        }

        public void UpdateOrder(int id, OrderStatus status, string userId)
        {
            repo.UpdateOrder(id, status, userId);
        }

        public bool CancelOrder(int id, string userId)
        {
            var result = repo.CancelOrder(id, userId);
            return result;
        }

        public Asset GetPDF(int id)
        {
            return repo.GetPDF(id);
        }

        public void SaveOrderPDFAsset(Order order, Asset pdf)
        {
            repo.SaveOrderPDF(order, pdf);
        }

        public Log SaveMessage(int orderId, string message, string userId)
        {
            return repo.SaveLogMessage(orderId, message, userId);
        }
    }
}