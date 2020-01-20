using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Utilities;
using OrderViewModel = FrogFoot.Areas.ISPAdmin.Models.OrderViewModel;

namespace FrogFoot.Repositories
{
    public class OrderRepository
    {
        private ApplicationDbContext db = Db.GetInstance();

        public Order GetOrder(int id)
        {
            var order = db.Orders
                .Include(o => o.Client)
                .Include(o => o.Logs)
                .Include("Logs.User")
                .Include("Client.Location")
                .Include("Client.Estate")
                .Include(o => o.ISP)
                .Include(o => o.ISP.ISPEstateDiscounts)
                .Include(o => o.ISPProduct)
                .Include(o => o.Special)
                .Include(o => o.PDFs)
                .Include(o => o.OrderFFProducts)
                .Include("OrderFFProducts.FFProduct")
                .FirstOrDefault(z => z.OrderId == id);
            return order;
        }

        public IQueryable<Order> GetOrders()
        {
            return db.Orders
                .Include(o => o.Client)
                .Include("Client.Location")
                .Include("Client.Estate")
                .Include(o => o.ISP)
                .Include(o => o.ISPProduct)
                .OrderByDescending(o => o.CreatedDate);
        }

        public List<FFProduct> GetFFProducts()
        {
            return db.FFProducts.ToList();
        }

        public List<ISPProduct> GetISPProductsByGridding(int locId, int? estateId, int? ispId)
        {
            var products = db.ISPProducts
                .Include(i => i.ISPLogo)
                .Include(p => p.ISP)
                .Where(p => !p.IsDeleted && p.IsActive &&
                            (estateId != null && p.ISPEstateProducts.Any(x => x.EstateId == estateId) || estateId == null && p.ISPLocationProducts.Any(x => x.LocationId == locId))
                            && (ispId == null || (ispId != null && p.ISPLocationProducts.Any(x => x.ISPId == ispId))))

                .ToList();

            return products;
        }

        public bool ClientCreateOrder(string userId, int prodId, string orderUrl, ContractTerm contractTerm)
        {
            try
            {
                var user = db.Users.Include(u => u.Location).Include(u => u.Estate).Include(u => u.Zone).FirstOrDefault(u => u.Id == userId);
                var product = db.ISPProducts.Include(p => p.ISP).First(p => p.ISPProductId == prodId);
                var special = GetSpecial(user.Zone, product.IsSpecial);

                var precinctCode = "";

                if (user.Estate != null && !string.IsNullOrEmpty(user.Estate.EstateCode))
                {
                    precinctCode = user.Estate.EstateCode;
                }
                else if (!string.IsNullOrEmpty(user.Location.PrecinctCode))
                {
                    precinctCode = user.Location.PrecinctCode;
                }

                var currentDateTime = DateTime.Now;

                var order = new Order
                {
                    ISPId = product.ISPId,
                    Client = user,
                    ClientId = userId,
                    Status = OrderStatus.New,
                    CreatedDate = currentDateTime,
                    CreatedByRole = "Client",
                    ISPProduct = product,
                    ISPProductId = product.ISPProductId,
                    IsSpecial = special != null,
                    Special = special,
                    ClientContractTerm = contractTerm,
                    ISPOrderNo = "Please provide",
                    FFOrderNo = "placeholder",
                    OrderFFProducts = new List<OrderFFProduct>
                    {
                        new OrderFFProduct
                        {
                            Quantity = 1,
                            FFProductId =
                                db.FFProducts.FirstOrDefault(p => p.LineSpeed == product.LineSpeed).FFProductId
                        }
                    },
                    StatusList = new List<Status>
                    {
                        new Status {OrderStatus = OrderStatus.New, TimeStamp = currentDateTime},
                    }
                };

                product.Orders.Add(order);
                db.Orders.Add(order);
                order.Logs.Add(new Log { UserId = userId, Type = UserAction.Create, OrderStatus = OrderStatus.New, TimeStamp = DateTime.Now });
                SaveChanges(db);
                    
                //once you have the orderId then use that as the unique order number
                order.FFOrderNo = string.Format("FTTH-{0}-{1}-{2:D6}", product.ISP.ISPCode, precinctCode, order.OrderId);
                SaveChanges(db);
                EmailSender.SendISPNewOrderNotification(order, product.ISP, user, orderUrl);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Order CreateOrder(OrderViewModel model, string role, string orderUrl, string userId)
        {
            var userPassword = "";

            //get existing user if exists
            var user = db.Users.Include(u => u.Location).Include(u => u.Estate).Include(u => u.Zone).FirstOrDefault(u => u.Email == model.Order.Client.Email);

            if (string.IsNullOrEmpty(model.Order.Client.Id) && user == null)
            {
                var newUser = model.Order.Client;
                var userRepo = new UserRepository();
                var result = userRepo.CreateUser(new User
                {
                    UserName = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Email = newUser.Email,
                    EmailConfirmed = true,
                    UserCreatedWithOrder = true,
                    PhoneNumber = newUser.PhoneNumber,
                    Landline = newUser.Landline,
                    CreatedDate = DateTime.Now,
                    Address = newUser.Address,
                    LocationId = model.Order.LocationId,
                    EstateId = model.Order.Client.EstateId,
                    Latitude = model.Order.Client.Latitude,
                    Longitude = model.Order.Client.Longitude,

                    IsOwner = model.Order.Client.IsOwner,
                    OwnerName = model.Order.Client.OwnerName,
                    OwnerPhoneNumber = model.Order.Client.OwnerPhoneNumber,
                    OwnerEmail = model.Order.Client.OwnerEmail,

                    HasAltContact = newUser.HasAltContact,
                    AltFirstName = newUser.AltFirstName,
                    AltLastName = newUser.AltLastName,
                    AltEmail = newUser.AltEmail,
                    AltCellNo = newUser.AltEmail,
                    AltLandline = newUser.AltLandline

                }, UserType.Client);
                user = db.Users.Include(u => u.Location).Include(u => u.Estate).Include(u => u.Zone).First(u => u.Id == result.User.Id);
                userPassword = result.UserPassword;
            }

            var product = db.ISPProducts.Include(i => i.ISP).First(i => i.ISPProductId == model.Order.ISPProductId);
            var precinctCode = "";
            var special = GetSpecial(user.Zone, product.IsSpecial);

            if (user.Estate != null && !string.IsNullOrEmpty(user.Estate.EstateCode))
            {
                precinctCode = user.Estate.EstateCode;
            }
            else if (!string.IsNullOrEmpty(user.Location.PrecinctCode))
            {
                precinctCode = user.Location.PrecinctCode;
            }

            var currentDateTime = DateTime.Now;

            var order = new Order
            {
                ISPId = product.ISPId,
                ClientId = user.Id,
                Status = OrderStatus.Ordered,
                ISPOrderNo = model.Order.ISPOrderNo ?? "Please provide",
                FFOrderNo = "placeholder",
                CreatedDate = currentDateTime,
                CreatedByRole = role,
                ISPProduct = product,
                ISPProductId = product.ISPProductId,
                IsSpecial = special != null,
                Special = special,
                ClientContractTerm = model.Order.ClientContractTerm,
                OrderFFProducts = new List<OrderFFProduct>
                    {
                        new OrderFFProduct
                        {
                            Quantity = 1,
                            FFProductId = db.FFProducts.FirstOrDefault(p => p.LineSpeed == product.LineSpeed).FFProductId
                        }
                    },
                StatusList = new List<Status>
                {
                    new Status {OrderStatus = OrderStatus.New, TimeStamp = currentDateTime},
                    new Status {OrderStatus = OrderStatus.Pending, TimeStamp = currentDateTime},
                    new Status {OrderStatus = OrderStatus.Ordered, TimeStamp = currentDateTime}
                }
            };

            //quantity items
            for (int i = 0; i < model.QuantityId.Count; i++)
            {
                if (model.QuantityQty[i] > 0)
                    order.OrderFFProducts.Add(new OrderFFProduct() { FFProductId = model.QuantityId[i], Quantity = model.QuantityQty[i] });
            }
            //checkbox items: if checked, save with qty=1
            int j = 0;
            for (int i = 0; i < model.OptionId.Count; i++)
            {
                if (model.Option[j])
                {
                    order.OrderFFProducts.Add(new OrderFFProduct() { FFProductId = model.OptionId[i], Quantity = 1 });
                    j++;
                }
                j++;
            }

            db.Orders.Add(order);
            order.Logs.Add(new Log { UserId = userId, Type = UserAction.Create, OrderStatus = order.Status, TimeStamp = DateTime.Now });
            SaveChanges(db);

            //once you have the orderId then use that as the unique order number
            order.FFOrderNo = string.Format("FTTH-{0}-{1}-{2:D6}", product.ISP.ISPCode, precinctCode, order.OrderId);
            SaveChanges(db);

            EmailSender.SendISPNewOrderNotification(order, product.ISP, user, orderUrl);
            EmailSender.SendRTNewOrderNotification(order.OrderId);

            //var cap = product.IsCapped ? product.Cap.ToString() : "";
            //var isCapped = product.IsCapped ? "Capped" : "Uncapped";

            //var email = new EmailDto
            //{
            //    Subject = "Order Confirmation",
            //    Body = "Dear " + user.FirstName + "," +
            //           "<br/><br/>This is a courtesy email to inform you that " + product.ISP.Name +
            //           " has placed an order for a Frogfoot Fibre link to your premises in order to supply you with a fibre broadband service." +
            //           "<br/><br/>If you did not request this service then please contact the ISP using the details below." +

            //           "<br/><br/>A user account has been created for you on the Frogfoot Fibre portal. " +
            //           "<br/>Should you wish to confirm your details and check for updates, please make use of the following login credentials." +

            //           "<br/><br/>URL: http://www.frogfootfibre.com" +
            //           "<br/><br/>Username: " + user.Email +
            //           "<br/><br/>Password: " + userPassword +

            //           "<br/><br/><br/><br/>If you have any queries about the broadband fibre offering, you are welcome to contact <strong>" + product.ISP.Name +
            //           "</strong> via email at " + product.ISP.EmailAddress + " or by phone on " + product.ISP.LandlineNo + "." +
            //           "<br/><br/>Warm regards" +
            //           "<br/><br/>The Frogfoot team." +
            //           "<br/><br/>ftth@frogfoot.com"
            //};

            //EmailSender.SendEmail(email, user);

            return order;
        }

        public void SaveOrderPDF(Order order, Asset pdf)
        {
            var orderToUpdate = db.Orders.FirstOrDefault(o => o.OrderId == order.OrderId);

            if (orderToUpdate != null)
            {
                pdf.OrderId = orderToUpdate.OrderId;
                db.Assets.Add(pdf);
                db.SaveChanges();
            }
        }

        public Asset GetPDF(int id)
        {
            return db.Assets.Find(id);
        }

        public Log SaveLogMessage(int orderId, string message, string userId)
        {
            var log = new Log
            {
                OrderId = orderId,
                UserId = userId,
                Message = message,
                Type = UserAction.Message,
                TimeStamp = DateTime.Now
            };
            log.User = db.Users.Find(userId);
            db.Logs.Add(log);
            db.SaveChanges();
            return log;
        }

        public bool CancelOrder(int id, string userId)
        {
            var order = db.Orders.Find(id);
            order.Status = OrderStatus.Canceled;
            order.Logs.Add(new Log { UserId = userId, Type = UserAction.Cancel, OrderStatus = order.Status, TimeStamp = DateTime.Now });
            order.StatusList.Add(new Status {OrderStatus = OrderStatus.Canceled, TimeStamp = DateTime.Now});
            SaveChanges(db);
            return false;
        }

        public void UpdateOrder(int id, OrderStatus status, string userId)
        {
            var order = db.Orders.Find(id);
            order.Status = status;
            var action = UserAction.Edit;
            if (status == OrderStatus.Canceled) { action = UserAction.Cancel; }
            order.Logs.Add(new Log { UserId = userId, Type = action, OrderStatus = status, TimeStamp = DateTime.Now });
            order.StatusList.Add(new Status {OrderStatus = status, TimeStamp = DateTime.Now});
            SaveChanges(db);
            EmailSender.SendRTNewOrderNotification(id);
        }

        public void SaveISPOrderNo(int orderId, string orderNo, string userId)
        {
            var order = db.Orders.Find(orderId);
            order.ISPOrderNo = orderNo;
            order.Logs.Add(new Log { UserId = userId, Type = UserAction.Edit, OrderStatus = order.Status, TimeStamp = DateTime.Now });
            SaveChanges(db);
        }

        public void EditOrder(int orderId, int ffProductId, int ispProductId, OrderStatus? status, string userId, ContractTerm clientContractTerm, List<FFProdEditDto> products)
        {
            var order = GetOrder(orderId);
            order.ISPProductId = ispProductId;
            order.ClientContractTerm = clientContractTerm;

            var action = UserAction.Edit;

            if (status != null)
            {
                order.Status = (OrderStatus)status;
                order.StatusList.Add(new Status { OrderStatus = (OrderStatus)status, TimeStamp = DateTime.Now });
            }

            //create defuaul line speed prod
            products.Add(new FFProdEditDto
            {
                action = true,
                id = ffProductId,
                qty = 1
            });

            //FFProdEditDto
            //true = add/update
            //false = delete
            //null = no change/do nothing
            foreach (var prod in products)
            {
                //add or update
                if (prod.action == true)
                {
                    //if prod exists then update it
                    if (order.OrderFFProducts.Any(p => p.FFProductId == prod.id))
                    {
                        var prodToUpdate = order.OrderFFProducts.First(p => p.FFProductId == prod.id);
                        prodToUpdate.Quantity = prod.qty;
                    }
                    else //if not exists then add it
                    {
                        order.OrderFFProducts.Add(new OrderFFProduct
                        {
                            OrderId = order.OrderId,
                            FFProductId = prod.id,
                            Quantity = prod.qty
                        });
                    }
                }
                db.SaveChanges();

                //remove
                if (prod.action == false)
                {
                    var orderFFProdToDelete = order.OrderFFProducts.FirstOrDefault(p => p.FFProductId == prod.id);
                    if (orderFFProdToDelete != null)
                    {
                        db.OrderFFProducts.Remove(orderFFProdToDelete);
                    }
                    db.SaveChanges();
                }
            }

            order.Logs.Add(new Log { UserId = userId, Type = action, OrderStatus = order.Status, TimeStamp = DateTime.Now });
            SaveChanges(db);

            EmailSender.SendRTNewOrderNotification(orderId);
        }

        private Special GetSpecial(Zone zone, bool? isSpecial)
        {
            if (zone != null && zone.AllowSpecial && isSpecial == true)
            {
                if (zone.Status == TrenchingStatus.Undefined)
                {
                    return db.Specials.FirstOrDefault(s => s.SpecialType == SpecialType.EarlyBird);
                }

                if (zone.Status == TrenchingStatus.HasDates)
                {
                    return db.Specials.FirstOrDefault(s => s.SpecialType == SpecialType.JustInTime);
                }
            }
            return null;
        }

        private static void SaveChanges(ApplicationDbContext context)
        {
            try
            {
                context.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                StringBuilder sb = new StringBuilder();

                foreach (var failure in ex.EntityValidationErrors)
                {
                    sb.AppendFormat("{0} failed validation\n", failure.Entry.Entity.GetType());
                    foreach (var error in failure.ValidationErrors)
                    {
                        sb.AppendFormat("- {0} : {1}", error.PropertyName, error.ErrorMessage);
                        sb.AppendLine();
                    }
                }

                throw new DbEntityValidationException(
                    "Entity Validation Failed - errors follow:\n" +
                    sb.ToString(), ex
                );
            }
        }
    }
}
