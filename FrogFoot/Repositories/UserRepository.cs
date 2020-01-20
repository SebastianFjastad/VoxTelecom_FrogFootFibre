using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;

namespace FrogFoot.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext db = Db.GetInstance();

        public UserViewModel CreateUser(User user, UserType type)
        {
            try
            {
                var manager = new UserManager<User>(new UserStore<User>(db));
                manager.UserValidator = new UserValidator<User>(manager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };

                var password = PasswordGenerator.Generate(6);
                var result = manager.Create(user, password);
                if (result.Succeeded)
                {
                    IdentityResult response = manager.AddToRole(user.Id, type.ToString());

                    if (user.IsChamp)
                    {
                        manager.AddToRole(user.Id, "Champ");
                    }

                    ZoneSync.ProcessUser(user.Id);

                    return new UserViewModel
                    {
                        User = db.Users.Find(user.Id),
                        UserPassword = password
                    };
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            return null;
        }

        public void EnableUser(User user)
        {
            var existingUser = db.Users.FirstOrDefault(u => u.Email == user.Email);

            if (existingUser != null)
            {
                existingUser.IsDeleted = false;
                existingUser.FirstName = existingUser.FirstName;
                existingUser.LastName = existingUser.LastName;
                existingUser.Address = existingUser.Address;
                existingUser.PhoneNumber = existingUser.PhoneNumber;
                existingUser.ISPId = existingUser.ISPId;
                db.SaveChanges();
            }
        }

        public bool CheckUserCanOrder(string userId)
        {
            var user = db.Users.Include(l => l.Location).Include(l => l.Zone).First(u => u.Id == userId);
            //if not in zone or zone does not allow ordering then return false
            return user.Zone != null && user.Zone.AllowOrder;
        }

        public void EditUser(User user)
        {
            try
            {
                var userToUpdate = db.Users.Find(user.Id);
                ZoneSync.CheckAndUpdateUserForZone(user, db);

                userToUpdate.FirstName = user.FirstName;
                userToUpdate.LastName = user.LastName;
                userToUpdate.PhoneNumber = user.PhoneNumber;
                userToUpdate.IsUserTypeAdmin = user.IsUserTypeAdmin;
                userToUpdate.ISPId = user.ISPId;
                userToUpdate.Latitude = user.Latitude;
                userToUpdate.Longitude = user.Longitude;
                userToUpdate.Address = user.Address;
                userToUpdate.LocationId = user.LocationId;
                userToUpdate.EstateId = user.EstateId;
                userToUpdate.EmailConfirmed = user.EmailConfirmed;
                userToUpdate.UsePrecinctCodeForChamp = user.UsePrecinctCodeForChamp;
                userToUpdate.ISPCommsOptOutStatus = user.ISPCommsOptOutStatus;
                userToUpdate.FFCommsOptOutStatus = user.FFCommsOptOutStatus;

                var manager = new UserManager<User>(new UserStore<User>(db));
                if (!userToUpdate.IsChamp && user.IsChamp) //if the existing user isn't in champ role but the edited user is - add to role
                {
                    manager.AddToRole(user.Id, "Champ");
                }
                else if (userToUpdate.IsChamp && !user.IsChamp) //if the existing user is champ role but the edited user isn't - remove from role
                {
                    manager.RemoveFromRole(user.Id, "Champ");
                }

                userToUpdate.IsChamp = user.IsChamp;

                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
        }

        public void EditUserFromOrder(User user)
        {
            try
            {
                var userToUpdate = db.Users.Find(user.Id);
                ZoneSync.CheckAndUpdateUserForZone(user, db);

                userToUpdate.FirstName = user.FirstName;
                userToUpdate.LastName = user.LastName;
                userToUpdate.PhoneNumber = user.PhoneNumber;
                userToUpdate.Landline = user.Landline;
                userToUpdate.Latitude = user.Latitude;
                userToUpdate.Longitude = user.Longitude;
                userToUpdate.Address = user.Address;

                userToUpdate.IsOwner = user.IsOwner;
                userToUpdate.OwnerName = user.OwnerName;
                userToUpdate.OwnerPhoneNumber = user.OwnerPhoneNumber;
                userToUpdate.OwnerEmail = user.OwnerEmail;

                if (user.HasAltContact)
                {
                    userToUpdate.AltFirstName = user.AltFirstName;
                    userToUpdate.AltLastName = user.AltLastName;
                    userToUpdate.AltCellNo = user.AltCellNo;
                    userToUpdate.AltLandline = user.AltLandline;
                    userToUpdate.AltEmail = user.AltEmail;
                }

                db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
        }

        public User GetUser(string userId)
        {
            db.Configuration.ProxyCreationEnabled = false;
            return db.Users.Include(u => u.ISP)
                .Include(u => u.Zone)
                .Include(u => u.Estate)
                .Include(u => u.Location)
                .Include(u => u.Orders).FirstOrDefault(u => u.Id == userId);
        }

        public IQueryable<User> GetUsers()
        {
            return db.Users
                .Include(u => u.ISP)
                .Include(u => u.Location)
                .Include(u => u.Estate)
                .Include(u => u.Zone)
                .Where(u => !u.IsDeleted);
        }

        public IQueryable<UserDto> GetUserDtos(Expression<Func<User, bool>> predicate, int? ispId = null)
        {
            return db.Users
                .Include(u => u.Location)
                .Include(u => u.Zone)
                .Include(u => u.Estate)
                .Include(u => u.ISP)
                .Include(u => u.Orders)
                .Include(u => u.ISPClientContacts)
                .Include(u => u.ClientContactMethods)
                .Where(predicate).Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Landline = u.Landline,
                    Address = u.Address,
                    LocationObj = u.Location,
                    UsePrecinctCodeForChamp = u.UsePrecinctCodeForChamp,
                    ZoneObj = u.Zone,
                    EstateObj = u.Estate,
                    OrdersObj = u.Orders,
                    ISPClientContactObj = ispId != null ? u.ISPClientContacts.FirstOrDefault(c => c.ISPId == ispId) : null,
                    ClientContactMethods = u.ClientContactMethods
                });
        }

        public List<User> GetUsersForMap()
        {
            //this is a bit of a hack
            //it should get all users in Client role but I was struggling to get that from Identity tables
            //or use the UserManager which was very slow
            return db.Users.Include(u => u.Orders).Where(u => u.ISPId == null && u.Latitude != null && u.Longitude != null && u.Address.Length > 1).ToList();
        }

        public List<User> GetUsersByEmail(string term)
        {
            var manager = new UserManager<User>(new UserStore<User>(db));
            var clients = db.Users.Where(u => u.Email.Contains(term) && !u.IsDeleted).ToList();
            return clients.Where(u => manager.IsInRole(u.Id, "Client")).ToList();
        }

        public void UpdateProfile(User user)
        {
            #region Client ISP Contacts
            //remove ISP Contacts that weren't selected
            user.ClientISPContacts = user.ClientISPContacts.Where(c => c.IsISPSelected).ToList();

            //find and remove all the clientIspContacts and replace them with the new ones
            var clientISPContacts = db.ClientISPContacts.Where(c => c.UserId == user.Id);
            if (clientISPContacts.Any())
                db.ClientISPContacts.RemoveRange(clientISPContacts);
            #endregion

            #region Client Contact Methods
            //remove ContactMethods that aren't selected        
            user.ClientContactMethods = user.ClientContactMethods.Where(x => x.IsSelected).ToList();

            //find and remove all the clientIspContacts and replace them with the new ones
            var contactMethods = db.ClientContactMethods.Where(x => x.UserId == user.Id);
            if (contactMethods.Any())
                db.ClientContactMethods.RemoveRange(contactMethods);
            #endregion

            var profile = db.Users.First(u => u.Id == user.Id);
            ZoneSync.CheckAndUpdateUserForZone(user, db);

            profile.FirstName = user.FirstName;
            profile.LastName = user.LastName;
            profile.PhoneNumber = user.PhoneNumber;
            profile.Landline = user.Landline;
            profile.Address = user.Address;
            profile.Latitude = user.Latitude;
            profile.Longitude = user.Longitude;
            profile.HasAltContact = user.HasAltContact;
            profile.LocationId = user.LocationId;
            profile.EstateId = user.EstateId;
            profile.ISPCommsOptOutStatus = user.ISPCommsOptOutStatus;
            profile.FFCommsOptOutStatus = user.FFCommsOptOutStatus;
            profile.ClientISPContacts = user.ClientISPContacts;
            profile.ClientContactMethods = user.ClientContactMethods;

            if (user.HasAltContact)
            {
                profile.AltFirstName = user.AltFirstName;
                profile.AltLastName = user.AltLastName;
                profile.AltEmail = user.AltEmail;
                profile.AltCellNo = user.AltCellNo;
                profile.AltLandline = user.AltLandline;
            }

            db.SaveChanges();
        }

        public void UpdateAltContact(User user)
        {
            var profile = db.Users.First(u => u.Id == user.Id);
            profile.HasAltContact = user.HasAltContact;
            profile.AltFirstName = user.AltFirstName;
            profile.AltLastName = user.AltLastName;
            profile.AltEmail = user.AltEmail;
            profile.AltCellNo = user.AltCellNo;
            profile.AltLandline = user.AltLandline;
            db.SaveChanges();
        }

        public void UpdateOwnerDetails(User user)
        {
            var profile = db.Users.First(u => u.Id == user.Id);
            profile.IsOwner = user.IsOwner;
            profile.OwnerName = user.OwnerName;
            profile.OwnerEmail = user.OwnerEmail;
            profile.OwnerPhoneNumber = user.OwnerPhoneNumber;
            db.SaveChanges();
        }

        public ClientOrderStatusDto GetUserOrderStatus(string email, decimal? latitude, decimal? longitude)
        {
            db.Configuration.ProxyCreationEnabled = false;

            var response = new ClientOrderStatusDto();
            var user = db.Users.Include(u => u.Location).Include(u => u.Zone).FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                ZoneSync.ProcessUser(user.Id);
                response.UserAllowedToOrder = user.Zone.AllowOrder;
                var latestOrder = db.Orders.Where(o => o.ClientId == user.Id && o.Status != OrderStatus.Canceled).OrderByDescending(o => o.CreatedDate).FirstOrDefault();
                response.UserHasOrder = latestOrder != null;
                if ((user.Location != null && !user.Location.AllowOrder) && (user.Zone != null && !user.Zone.AllowOrder))
                {
                    response.UserAllowedToOrder = false;
                }
                response.Client = user;
                response.Client.Orders = null; //for circular json serialization
                var zone = user.Zone;
                if (zone != null && zone.AllowSpecial)
                {
                    //check if zone special applies
                    switch (zone.Status)
                    {
                        case TrenchingStatus.Undefined:
                            response.Special = db.Specials.First(s => s.SpecialType == SpecialType.EarlyBird);
                            break;
                        case TrenchingStatus.HasDates:
                            response.Special = db.Specials.First(s => s.SpecialType == SpecialType.JustInTime);
                            break;
                    }
                }
            }
            else if (latitude != null && longitude != null)
            {
                var zones = db.Zones.Where(z => !z.IsDeleted);

                string lat = latitude.ToString().Replace(",", ".").Replace("-", "");
                string lng = longitude.ToString().Replace(",", ".").Replace("-", "");

                const string url = "http://maps.frogfoot.net/ftth/check";
                using (var client = new WebClient())
                {
                    string result = client.DownloadString(url + string.Format("?ll=-{0},{1}", lat, lng));

                    //if there is a zone that matches the zone code of the returned result the assign that zone to the user then save.
                    dynamic data = JObject.Parse(result);
                    string precinctCode = data["precinct-zone"];

                    if (data["possible"] == true && !string.IsNullOrEmpty(precinctCode))
                    {
                        var zone = zones.FirstOrDefault(z => z.Code == precinctCode);
                        if (zone != null)
                        {
                            response.UserAllowedToOrder = zone.AllowOrder;

                            //check if zone special applies
                            if (zone.AllowSpecial)
                            {
                                switch (zone.Status)
                                {
                                    case TrenchingStatus.Undefined:
                                        response.Special = db.Specials.First(s => s.SpecialType == SpecialType.EarlyBird);
                                        break;
                                    case TrenchingStatus.HasDates:
                                        response.Special = db.Specials.First(s => s.SpecialType == SpecialType.JustInTime);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            return response;
        }

        public IQueryable<User> GetUsersByGridding(string precinctCode, int? locationId, int? zoneId, string userId)
        {
            string _precinctCode = precinctCode;
            int? _zoneId = zoneId;
            
            IQueryable<User> users;

            if (!string.IsNullOrEmpty(_precinctCode))
            {
                users = (from u in db.Users
                    .Include(u => u.Orders)
                    .Include(u => u.Location)
                    .Include(u => u.Zone)
                         where !u.IsDeleted && u.Location.PrecinctCode == _precinctCode
                         select u);
            }
            else
            {
                users = (from u in db.Users
                        .Include(u => u.Orders)
                        .Include(u => u.Zone)
                         where !u.IsDeleted
                               && (_zoneId != null && u.ZoneId == _zoneId)
                         select u);
            }

            return users;
        }

        public IQueryable<User> GetUsersByGriddingForChamp(string userId)
        {
            var user = db.Users.Include(u => u.Location).FirstOrDefault(u => u.Id == userId);
            string precinctCode = "";
            int? estateId = null;

            if (user.UsePrecinctCodeForChamp || user.EstateId == null)
            {
                precinctCode = user.Location.PrecinctCode;
            }
            else
            {
                estateId = user.EstateId;
            }

            IQueryable<User> usersFilterQuery = db.Users
                        .Include(u => u.Orders)
                        .Include(u => u.Zone)
                        .Where(u => !u.IsDeleted);

            if (!string.IsNullOrEmpty(precinctCode))
            {
                usersFilterQuery = usersFilterQuery.Where(u => u.Location.PrecinctCode == precinctCode);
            }
            else
            {
                usersFilterQuery = usersFilterQuery.Where(u => u.EstateId == estateId);
            }

            return usersFilterQuery;
        }

        public void OptOutFFComms(string id)
        {
            var user = db.Users.Find(id);

            if (user != null)
            {
                user.FFCommsOptOutStatus = true;
                db.SaveChanges();
            }
        }

        public void OptOutISPComms(string id)
        {
            var user = db.Users.Find(id);

            if (user != null)
            {
                user.ISPCommsOptOutStatus = true;
                db.SaveChanges();
            }
        }

        public bool DeleteUser(string id)
        {
            var user = db.Users.Find(id);
            user.IsDeleted = true;
            db.SaveChanges();
            return false;
        }

        public List<ClientISPContact> GetClientISPContacts(string userId)
        {
            return db.ClientISPContacts.Where(u => u.UserId == userId).ToList();
        }

        public List<ClientContactMethod> GetClientContactMethods(string userId)
        {
            return db.ClientContactMethods.Where(u => u.UserId == userId).ToList();
        }

        public void SaveISPClientContacts(int[] ids, string userId)
        {
            var user = db.Users.Include(u => u.ClientISPContacts).FirstOrDefault(u => u.Id == userId);

            db.ClientISPContacts.RemoveRange(db.ClientISPContacts.Where(u => u.UserId == userId));

            user.ClientISPContacts = ids.Select(id => new ClientISPContact { UserId = userId, ISPId = id, IsISPSelected = true }).ToList();

            db.SaveChanges();
        }

        public List<ContactMethod> GetContactMethods()
        {
            return db.ContactMethods.ToList();
        }

        public ISPContactAuditViewModel GetUserForClientContact(string email, string cellNo, string landline)
        {
            var response = new ISPContactAuditViewModel();
            var userQuery = db.Users.Include(u => u.Orders).Include(u => u.ISPClientContacts);
            User user = null;

            if (!string.IsNullOrEmpty(email)) //if email
            {
                user = userQuery.FirstOrDefault(u => u.Email.Contains(email));
            }
            else if (!string.IsNullOrEmpty(cellNo)) //if cell no
            {
                user = userQuery.FirstOrDefault(u => u.PhoneNumber.Contains(cellNo));
            }
            else if (!string.IsNullOrEmpty(landline)) //if landline
            {
                user = userQuery.FirstOrDefault(u => u.Landline.Contains(landline));
            }

            //if the user is found then fetch the related entities. The entity mapping doesn't seem to work for this so have to do it manually.
            if (user != null)
            {
                response.Client = user;
                response.ISPClientContacts = db.ISPClientContacts.Where(c => c.ClientId == user.Id).ToList();
                if (response.ISPClientContacts != null && response.ISPClientContacts.Any())
                {
                    foreach (var ispClientContact in response.ISPClientContacts)
                    {
                        response.ISPUsers.Add(db.Users.Include(i => i.ISP).First(u => u.Id == ispClientContact.ISPUserId));
                    }
                }
            }

            return response;
        }
    }
}
