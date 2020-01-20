using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Repositories;

namespace FrogFoot.Services
{
    public class AdminService
    {
        private UserRepository userRepo = new UserRepository();
        private ISPRepository ispRepo = new ISPRepository();
        private OrderRepository orderRepo = new OrderRepository();
        private GriddingRepository griddingRepo = new GriddingRepository();
        private ReportRepository reportRepo = new ReportRepository();
        private PostRepository postRepo = new PostRepository();
        private PortalRepository portalRepo = new PortalRepository();

        #region Users
        public User GetUser(string clientId)
        {
            return userRepo.GetUser(clientId);
        }

        public IQueryable<User> GetUsers()
        {
            return userRepo.GetUsers();
        }

        public IQueryable<UserDto> GetUserDtos(Expression<Func<User, bool>> predicate)
        {
            return userRepo.GetUserDtos(predicate);
        }

        public List<User> SearchClient(string term)
        {
            return userRepo.GetUsersByEmail(term);
        }

        public void EditUser(User user)
        {
            userRepo.EditUser(user);
        }

        public void EditUserFromOrder(User user)
        {
            userRepo.EditUserFromOrder(user);
        }

        public UserViewModel DeleteUser(string id)
        {
            return new UserViewModel { HasErrors = userRepo.DeleteUser(id) };
        }

        public ISPContactAuditViewModel GetUserByParams(string email, string cellNo, string landline)
        {
            return userRepo.GetUserForClientContact(email, cellNo, landline);
        }
        #endregion

        #region ISPs
        public ISP GetISP(int ispId)
        {
            return ispRepo.GetISP(ispId);
        }

        public List<ISP> GetISPs()
        {
            return ispRepo.GetISPs().ToList();
        }

        public ISPViewModel SaveISP(ISP model)
        {
            var hasError = ispRepo.SaveISP(model);
            return new ISPViewModel
            {
                Message = hasError ? "There was an error saving the ISP" : "ISP saved successfully"
            };
        }

        public ISPViewModel DeleteISP(int ispId)
        {
            var hasError = ispRepo.DeleteISP(ispId);
            return new ISPViewModel
            {
                Message = hasError ? "There was an error deleting ISP" : "ISP deleted successfully"
            };
        }
        #endregion

        #region Locations
        public List<Location> GetLocations()
        {
            return griddingRepo.GetLocations()
                .Include(l => l.Estates)
                .Include(l => l.ISPLocationProducts).ToList()
                .Select(l => new Location
                {
                    Estates = l.Estates.Where(e => !e.IsDeleted).ToList(),
                    LocationId = l.LocationId,
                    Name = l.Name,
                    APIName = l.APIName,
                    PrecinctCode = l.PrecinctCode,
                    Residents = l.Residents,
                    AllowOrder = l.AllowOrder,
                    IsActive = l.IsActive,
                    ISPLocationProducts = l.ISPLocationProducts
                }).ToList();
        }

        public List<Estate> GetEstates()
        {
            return griddingRepo.GetEstates().ToList();
        }

        public List<Estate> GetEstatesAndLocation(int locId)
        {
            return griddingRepo.GetEstates().Include(e => e.Location).Where(e => e.LocationId == locId).ToList();
        } 

        public Estate GetEstateDiscounts(int estateId)
        {
            return
                griddingRepo.GetEstate()
                    .Include(e => e.ISPEstateDiscounts)
                    .Include("ISPEstateDiscounts.ISP")
                    .First(e => e.EstateId == estateId);
        }

        public void SaveDiscount(int? discountId, int estateId, int ispId, Discount discount)
        {
            griddingRepo.SaveDiscount(discountId, estateId, ispId, discount);
        }

        public void SaveLocation(string name, string apiName, string code, bool active, bool allowOrder, int residents)
        {
            griddingRepo.SaveLocation(name, apiName, code, active, allowOrder, residents);
            var cache = HttpContext.Current.Cache;
            if (cache["Locations"] != null)
                cache.Remove("Locations");
        }

        public void UpdateLocation(int id, string name, string apiName, string code, bool active, bool allowOrder, int residents)
        {
            griddingRepo.UpdateLocation(id, name, apiName, code, active, allowOrder, residents);
            var cache = HttpContext.Current.Cache;
            if (cache["Locations"] != null)
                cache.Remove("Locations");
        }

        public void DeleteLocation(int locId)
        {
            griddingRepo.DeleteLocation(locId);
            var cache = HttpContext.Current.Cache;
            if (cache["Locations"] != null)
                cache.Remove("Locations");
        }

        public void SaveEstate(int locationId, string name, string code)
        {
            griddingRepo.SaveEstate(locationId, name, code);
        }

        public void UpdateEstate(int id, int locationId, string name, string code)
        {
            griddingRepo.UpdateEstate(id, locationId, name, code);
        }

        public void DeleteEstate(int id)
        {
            griddingRepo.DeleteEstate(id);
        }

        #endregion

        #region Zones
        public List<Zone> GetZones()
        {
            return griddingRepo.GetZones().ToList();
        }

        public Zone GetZone(int zoneId)
        {
            return griddingRepo.GetZone(zoneId);
        }

        public void CreateZone(Zone zone)
        {
            griddingRepo.CreateZone(zone);
        }

        public void UpdateZone(Zone zone)
        {
            griddingRepo.UpdateZone(zone);
        }

        public void DeleteZone(int zoneId)
        {
            griddingRepo.DeleteZone(zoneId);
        }
        #endregion

        #region Gridding
        public int? UpdateProductLocationGridding(int prodId, int locId, int ispId, int? prodGridId)
        {
            return griddingRepo.UpdateProductLocationGridding(prodId, locId, ispId, prodGridId);
        }

        public int? UpdateProductEstateGridding(int prodId, int estId, int ispId, int? prodGridId)
        {
            return griddingRepo.UpdateProductEstateGridding(prodId, estId, ispId, prodGridId);
        }

        public List<PrecinctDto> GetPrecincts()
        {
            return griddingRepo.GetPrecints();
        }

        #endregion

        #region Interest
        public UserInterestViewModel GetUsersWithOrderStatus()
        {
            return new UserInterestViewModel
            {
                Users = userRepo.GetUsersForMap(),
                UsersAndOrders = reportRepo.GetAllUsersWithOrders(),
                Locations = griddingRepo.GetLocations().ToList(),
                Zones = griddingRepo.GetZones().ToList()
            };
        }

        public UserInterestViewModel GetUserMapUsers()
        {
            return new UserInterestViewModel()
            {
                Users = userRepo.GetUsersForMap(),
                Locations = griddingRepo.GetLocations().ToList(),
                Zones = griddingRepo.GetZones().ToList()
            };
        }
        #endregion

        #region Orders
        public Order GetOrder(int id)
        {
            return orderRepo.GetOrder(id);
        }

        public List<Order> GetOrders()
        {
            return orderRepo.GetOrders().ToList();
        }

        public OrderViewModel GetDataForOrderCreate()
        {
            var model = new OrderViewModel
            {
                Locations = griddingRepo.GetLocations().ToList(),
                Estates = griddingRepo.GetEstates().ToList(),
                Products = orderRepo.GetFFProducts(),
                ISPProducts = ispRepo.GetProducts().ToList()
            };

            return model;
        }

        public List<ISPProduct> GetGriddedProducts(int locId, int? estateId, int? ispId)
        {
            return orderRepo.GetISPProductsByGridding(locId, estateId, ispId);
        }

        public void SaveOrder(OrderViewModel model, string role, string orderUrl, string userId)
        {
            //convert to correct model for order save
            var orderVM = new Areas.ISPAdmin.Models.OrderViewModel
            {
                Order = model.Order,
                QuantityId = model.QuantityId,
                QuantityQty = model.QuantityQty,
                OptionId = model.OptionId,
                Option = model.Option
            };
            orderRepo.CreateOrder(orderVM, role, orderUrl, userId);
        }

        public void EditOrder(int orderid, int ffProductId, int ispProductId, OrderStatus status, string userId, ContractTerm clientContractTerm, List<FFProdEditDto> products)
        {
            orderRepo.EditOrder(orderid, ffProductId, ispProductId, status, userId, clientContractTerm, products);
        }

        public void UpdateStatus(int orderId, OrderStatus status, string userId)
        {
            orderRepo.UpdateOrder(orderId, status, userId);
        }

        public void CancelOrder(int orderId, string userId)
        {
            orderRepo.CancelOrder(orderId, userId);
        }

        public void SaveOrderPDFAsset(Order order, Asset pdf)
        {
            orderRepo.SaveOrderPDF(order, pdf);
        }

        public Asset GetPDF(int id)
        {
            return orderRepo.GetPDF(id);
        }

        public Log SaveMessage(int orderId, string message, string userId)
        {
            return orderRepo.SaveLogMessage(orderId, message, userId);
        }
        public ClientOrderStatusDto GetUserOrderStatus(string email, decimal? lat, decimal? lng)
        {
            return userRepo.GetUserOrderStatus(email, lat, lng);
        }

        #endregion

        #region ISPProducts

        public ISPProduct GetProduct(int prodId)
        {
            return ispRepo.GetProduct(prodId);
        }

        public List<ISPProduct> GetProducts(int ispId)
        {
            return ispRepo.GetProductsForISP(ispId);
        }

        #endregion

        #region Reports
        public List<ReportDataDto> GetAdminSalesReports(ReportViewModel model)
        {
            return reportRepo.GetAdminReports(model.LocationId, model.From, model.To);
        }

        public List<ReportDataDto> GetUsersReport()
        {
            return reportRepo.GetUsersReport();
        }

        public ReportViewModel GetSalesReport()
        {
            return reportRepo.GetSalesReport();
        }

        public List<ReportDataDto> GetISPUsersData()
        {
            return reportRepo.GetISPUsersData();
        }
        #endregion

        #region Posts
        public List<Post> GetPosts()
        {
            return postRepo.GetPosts().ToList();
        }

        public Post GetPost(int postId)
        {
            return postRepo.GetPost(postId);
        }

        public void CreatePost(Post post, HttpPostedFileBase upload)
        {
            postRepo.Create(post, upload, null, "Admin");
        }

        public void EditPost(Post post, HttpPostedFileBase upload)
        {
            postRepo.Edit(post, upload);
        }

        public void DeletePost(int postId)
        {
            postRepo.Delete(postId);
        }
        #endregion

        #region Portals

        public List<Portal> GetAllPortals()
        {
            return portalRepo.GetPortals();
        }

        public Portal GetPortal(int id)
        {
            return portalRepo.GetPortal(id);
        }

        public void SavePortal(Portal portal, HttpPostedFileBase img)
        {
            portalRepo.Save(portal, img);
            var cache = HttpContext.Current.Cache;
            if (cache["Portals"] != null)
                cache.Remove("Portals");
        }

        public void DeletePortal(int id)
        {
            portalRepo.Delete(id);
            var cache = HttpContext.Current.Cache;
            if (cache["Portals"] != null)
                cache.Remove("Portals");
        }

        public void DeleteUrl(int urlId)
        {
            portalRepo.DeleteUrl(urlId);
            var cache = HttpContext.Current.Cache;
            if (cache["Urls"] != null)
                cache.Remove("Urls");
        }

        #endregion

    }
}