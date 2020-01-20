using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Repositories;

namespace FrogFoot.Services
{
    public class ClientService
    {
        private ISPRepository ispRepo = new ISPRepository();
        private OrderRepository orderRepo = new OrderRepository();
        private UserRepository userRepo = new UserRepository();
        private PostRepository postRepo = new PostRepository();
        private PortalRepository portalRepo = new PortalRepository();
        private GriddingRepository griddingRepo = new GriddingRepository();

        #region Products
        public List<ISPProduct> GetProducts(string userId)
        {
            return ispRepo.GetProductsForClient(userId);
        }

        public List<ISPProduct> GetProductsForPublic(string precinctCode)
        {
            return ispRepo.GetProductsForPublic(precinctCode);
        }

        public List<int> GetGriddedProductIds(int? locId, int? estateId, int? ispId, bool? capped, bool? isM2MClientContract)
        {
            return ispRepo.GetGriddedProductIds(locId, estateId, ispId, capped, isM2MClientContract);
        }

        public ISPProduct GetProduct(int prodId)
        {
            return ispRepo.GetProduct(prodId);
        }

        public List<Special> GetSpecials()
        {
            return ispRepo.GetSpecials();
        }
        #endregion

        #region Gridding
        public List<Location> GetLocations()
        {
            List<Location> locations = HttpContext.Current.Cache["Locations"] as List<Location>;
            if (locations == null)
            {
                locations = griddingRepo.GetLocations().Where(l => l.IsActive).OrderBy(l => l.Name).ToList();
                locations.Insert(0, new Location
                {
                    LocationId = -1,
                    Name = "Other",
                    APIName = ""
                });

                HttpContext.Current.Cache["Locations"] = locations;
            }
            return locations;
        }

        public List<Portal> GetPortals()
        {
            List<Portal> portals = HttpContext.Current.Cache["Portals"] as List<Portal>;
            if (portals == null)
            {
                portals = portalRepo.GetPortals();
                HttpContext.Current.Cache["Portals"] = portals;
            }
            return portals;
        }

        public List<Url> GetUrls()
        {
            List<Url> urls = HttpContext.Current.Cache["Urls"] as List<Url>;
            if (urls == null)
            {
                urls = portalRepo.GetUrls();
                HttpContext.Current.Cache["Urls"] = urls;
            }
            return urls;
        }

        public List<Estate> GetEstates()
        {
            return griddingRepo.GetEstates().ToList();
        }

        public List<Zone> GetZonesForMaps()
        {
            return griddingRepo.GetZones().ToList();
        }

        public bool CheckPrecinctHasZones(string precinctCode)
        {
            return griddingRepo.GetZones().Any(z => z.PrecinctCode == precinctCode);
        }

        public bool CheckUserCanOrder(string userId)
        {
            return userRepo.CheckUserCanOrder(userId);
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
        #endregion

        #region Orders
        public List<Order> GetClientOrders(string userId)
        {
            return orderRepo.GetOrders()
                .Include(o => o.ISP.ISPEstateDiscounts)
                .Where(o => o.ClientId == userId && o.Status != OrderStatus.Canceled).ToList();
        }

        public bool PlaceOrder(string userId, int prodId, string orderUrl, ContractTerm contractTerm)
        {
            return orderRepo.ClientCreateOrder(userId, prodId, orderUrl, contractTerm);
        }
        #endregion

        #region Client
        public void UpdateProfile(User user)
        {
            userRepo.UpdateProfile(user);
        }

        public void UpdateAltContact(User user)
        {
            userRepo.UpdateAltContact(user);
        }

        public void UpdateOwnerDetails(User user)
        {
            userRepo.UpdateOwnerDetails(user);
        }

        public void EnableUser(User user)
        {
            userRepo.EnableUser(user);
        }

        public void OptOutISPComms(string id)
        {
            userRepo.OptOutISPComms(id);
        }

        public void OptOutFFComms(string id)
        {
            userRepo.OptOutFFComms(id);
        }

        public User GetUser(string id)
        {
            return userRepo.GetUser(id);
        }

        public List<ClientISPContact> GetClientISPContact(string userId)
        {
            return userRepo.GetClientISPContacts(userId);
        }

        public List<ClientContactMethod> GetClientContactMethods(string userId)
        {
            return userRepo.GetClientContactMethods(userId);
        } 

        public List<ContactMethod> GetContactMethods()
        {
            return userRepo.GetContactMethods();
        } 

        public void SaveClientISPContacts(int[] ids, string userId)
        {
            userRepo.SaveISPClientContacts(ids, userId);
        }

        #endregion

        #region Posts

        public List<Post> GetPosts(string userId, PostType? type, int[] postIdsToExclude, int pageNo = 1, int pageSize = 5)
        {
            return postRepo.GetPostsForClient(userId, type, postIdsToExclude, pageNo, pageSize);
        }

        public List<Post> GetPosts(string userId, PostType? type, int pageNo = 1, int pageSize = 5)
        {
            return postRepo.GetPostsForClient(userId, type, new int[0], pageNo, pageSize);
        }

        public Post GetPost(int postId)
        {
            return postRepo.GetPost(postId);
        }
        #endregion
    }
}