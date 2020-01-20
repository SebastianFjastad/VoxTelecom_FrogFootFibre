using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Areas.ISPAdmin.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Repositories;
using OrderViewModel = FrogFoot.Areas.ISPAdmin.Models.OrderViewModel;
using ReportViewModel = FrogFoot.Areas.ISPAdmin.Models.ReportViewModel;

namespace FrogFoot.Services
{
    public class ISPService
    {
        private OrderRepository orderRepo = new OrderRepository();
        private ISPRepository ispRepo = new ISPRepository();
        private UserRepository userRepo = new UserRepository();
        private GriddingRepository griddingRepo = new GriddingRepository();
        private ReportRepository reportRepo = new ReportRepository();

        #region Orders
        public Order GetOrder(int id)
        {
            return orderRepo.GetOrder(id);
        }

        public OrderViewModel GetDataForOrderCreate(string userId)
        {
            var user = userRepo.GetUser(userId);

            var model = new OrderViewModel
            {
                Locations = griddingRepo.GetLocations().Where(l => l.IsActive).ToList(),
                Estates = griddingRepo.GetEstates().ToList(),
                Products = orderRepo.GetFFProducts(),
                ISPProducts = ispRepo.GetProducts().Where(u => u.ISPId == user.ISPId).ToList(),
                Discounts = ispRepo.GetEstateDiscounts(userId)
            };

            return model;
        }

        public List<Order> GetOrders(string userId)
        {
            var user = userRepo.GetUser(userId);
            return orderRepo.GetOrders().Where(o => o.ISPId == user.ISPId).ToList();
        }

        public Order SaveOrder(OrderViewModel model, string role, string orderUrl, string userId)
        {
            return orderRepo.CreateOrder(model, role, orderUrl, userId);
        }

        public Asset GetPDF(int id)
        {
            return orderRepo.GetPDF(id);
        }

        public void EditOrder(int orderid, int ffProductId, int ispProductId, string userId, ContractTerm clientContractTerm, List<FFProdEditDto> ffProducts)
        {
            orderRepo.EditOrder(orderid, ffProductId, ispProductId, null, userId, clientContractTerm, ffProducts);
        }

        public void UpdateStatus(int orderId, OrderStatus status, string userId)
        {
            orderRepo.UpdateOrder(orderId, status, userId);
        }

        public void SaveISPOrderNo(int orderId, string orderNo, string userId)
        {
            orderRepo.SaveISPOrderNo(orderId, orderNo, userId);
        }

        public OrderViewModel CancelOrder(int id, string userId)
        {
            var hasError = orderRepo.CancelOrder(id, userId);
            return new OrderViewModel
            {
                Message = hasError ? "There was an error saving the order" : "Order saved successfully"
            };
        }

        public Log SaveMessage(int orderId, string message, string userId)
        {
            return orderRepo.SaveLogMessage(orderId, message, userId);
        }
        #endregion

        #region ISPProducts

        public List<ISPProduct> GetProducts(string userId, int? ispId)
        {
            return ispRepo.GetProductsByISPUserId(userId, ispId);
        }

        public ISPProduct GetProduct(int prodId)
        {
            return ispRepo.GetProduct(prodId);
        }

        public List<ISPProduct> GetGriddedProducts(int locId, int? estateId, int? ispId)
        {
            return orderRepo.GetISPProductsByGridding(locId, estateId, ispId);
        }

        public void SaveProduct(ISPProduct model, HttpPostedFileBase upload)
        {
            ispRepo.SaveProduct(model, upload);
        }

        public void EditProduct(ISPProduct model, HttpPostedFileBase upload)
        {
            ispRepo.EditProduct(model, upload);
        }

        public void SaveProductsStatus(List<ProductActiveDto> products)
        {
            ispRepo.SaveProductsStatus(products);
        }

        public void DeleteProduct(string userId, int prodId, int ispId)
        {
            ispRepo.ISPDeleteProduct(userId, prodId, ispId);
        }
        #endregion

        #region Gridding
        public List<Location> GetLocations()
        {
            return griddingRepo.GetLocations().Include(l => l.Estates).Include(l => l.ISPLocationProducts).ToList().Select(l => new Location
            {
                Estates = l.Estates.Where(e => !e.IsDeleted).ToList(),
                LocationId = l.LocationId,
                Name = l.Name,
                IsActive = l.IsActive,
                PrecinctCode = l.PrecinctCode,
                AllowOrder = l.AllowOrder,
                ISPLocationProducts = l.ISPLocationProducts
            }).ToList();
        }

        public List<Estate> GetEstatesAndLocations(int locId)
        {
            return griddingRepo.GetEstates().Include(e => e.Location).Where(e => e.LocationId == locId).ToList();
        }

        public int? UpdateProductLocationGridding(int prodId, int locId, int ispId, int? prodGridId)
        {
            return griddingRepo.UpdateProductLocationGridding(prodId, locId, ispId, prodGridId);
        }

        public int? UpdateProductEstateGridding(int prodId, int estId, int ispId, int? prodGridId)
        {
            return griddingRepo.UpdateProductEstateGridding(prodId, estId, ispId, prodGridId);
        }

        #endregion

        #region Users
        public User GetISPUser(string userId)
        {
            return userRepo.GetUser(userId);
        }

        public List<User> GetISPUsers(string userId)
        {
            var user = userRepo.GetUser(userId);
            return userRepo.GetUsers().Where(u => u.ISPId == user.ISPId).ToList();
        }

        public void EditUser(User user)
        {
            userRepo.EditUser(user);
        }

        public void DeleteUser(string userId)
        {
            userRepo.DeleteUser(userId);
        }

        public IQueryable<UserDto> GetUserDtos(Expression<Func<User, bool>> predicate, int? ispId = null)
        {
            return userRepo.GetUserDtos(predicate, ispId);
        }
        #endregion

        #region Client
        public List<User> SearchClient(string term)
        {
            return userRepo.GetUsersByEmail(term);
        }

        public User GetClient(string clientId)
        {
            return userRepo.GetUser(clientId);
        }

        public void EditClient(User user)
        {
            userRepo.EditUserFromOrder(user);
        }

        public ClientOrderStatusDto GetUserOrderStatus(string email, decimal? lat, decimal? lng)
        {
            return userRepo.GetUserOrderStatus(email, lat, lng);
        }

        public void CreateISPClientContact(string ispUserId, string clientId, int ispId)
        {
            ispRepo.CreateISPClientContact(ispUserId, clientId, ispId);
        }

        public void MakeISPContact(string clientId, int ispId)
        {
            ispRepo.MakeContact(clientId, ispId);
        }

        #endregion

        #region Leads
        public IQueryable<User> GetUsers()
        {
            return userRepo.GetUsers();
        } 
        #endregion

        #region Reports
        public List<ReportDataDto> GetReports(ReportViewModel model, string userId)
        {
            return reportRepo.GetISPReports(userId, model.LocationId, model.From, model.To);
        }
        #endregion
    }
}