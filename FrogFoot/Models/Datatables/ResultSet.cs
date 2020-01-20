using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace FrogFoot.Models.Datatables
{
    public class ResultSet
    {
        public T GetResult<T>(string search, string sortOrder, int start, int length, List<UserDto> dtResult, List<string> columnFilters, UserFilterType filterType)
        {
            switch (filterType)
            {
                case UserFilterType.UserTable: return (T)UserTableFilterResult(search, dtResult, columnFilters).SortBy(sortOrder).Skip(start).Take(length);
                case UserFilterType.UserMap: return (T)UserMapFilterResult(search, dtResult, columnFilters).SortBy(sortOrder).Skip(start).Take(length);
                case UserFilterType.UserLeads: return (T)UserLeadsFilterResult(search, dtResult, columnFilters).SortBy(sortOrder).Skip(start).Take(length);
                default:
                    throw new NotImplementedException("No result set type (enum) passed in");
            }
        }

        public int Count(string search, List<UserDto> dtResult, List<string> columnFilters, UserFilterType filterType)
        {
            switch (filterType)
            {
                case UserFilterType.UserTable:
                    return UserTableFilterResult(search, dtResult, columnFilters).Count();
                case UserFilterType.UserMap:
                    return UserMapFilterResult(search, dtResult, columnFilters).Count();
                case UserFilterType.UserLeads:
                    return UserLeadsFilterResult(search, dtResult, columnFilters).Count();
                default:
                    throw new NotImplementedException("No result set type (enum) passed in");
            }
        }

        private IQueryable<UserDto> UserTableFilterResult(string search, List<UserDto> dtResult, List<string> columnFilters)
        {
            return dtResult.AsQueryable().Where(u =>
                search == null ||
                u.FirstName != null && u.FirstName.ToLower().Contains(search.ToLower())
                || u.LastName != null && u.LastName.ToLower().Contains(search.ToLower())
                || u.Email != null && u.Email.ToLower().Contains(search.ToLower())
                || u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(search.ToLower())
                );
        }

        private IQueryable<UserDto> UserMapFilterResult(string search, List<UserDto> dtResult, List<string> columnFilters)
        {
            var filteredItems = dtResult.Where(u =>
                search == null ||
                u.FirstName != null && u.FirstName.ToLower().Contains(search.ToLower())
                || u.LastName != null && u.LastName.ToLower().Contains(search.ToLower())
                || u.Email != null && u.Email.ToLower().Contains(search.ToLower())
                || u.PhoneNumber != null && u.PhoneNumber.ToLower().Contains(search.ToLower())
                || u.Zone != null && u.ZoneObj.Code.ToLower().Contains(search.ToLower())
                ).ToList();

            return filteredItems.AsQueryable().Select(user => new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Landline = user.Landline,
                Address = user.Address,
                Zone = user.ZoneObj != null ? user.ZoneObj.Code : "",
                Ordered = user.OrdersObj.Any(o => o.Status != OrderStatus.Canceled) ? "Yes" : "No"
            });
        }

        private IQueryable<UserDto> UserLeadsFilterResult(string search, List<UserDto> dtResult, List<string> columnFilters)
        {
            return dtResult.AsQueryable().Where(u => (
                search == null ||
                u.FirstName != null && u.FirstName.ToLower().Contains(search.ToLower())
                 || u.LastName != null && u.LastName.ToLower().Contains(search.ToLower())
                 || u.Address != null && u.Address.ToLower().Contains(search.ToLower())
                 || u.LocationObj != null && u.LocationObj.Name.ToLower().Contains(search.ToLower())
                 || u.ZoneObj != null && u.ZoneObj.Code.ToLower().Contains(search.ToLower())
                 || u.Zone != null && u.ZoneObj.Status.ToString().ToLower().Contains(search.ToLower()))
                ).Select(user => new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Address = user.Address,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Landline = user.Landline,
                    Precinct = user.LocationObj != null ? user.LocationObj.Name : "",
                    Zone = user.ZoneObj != null ? user.ZoneObj.Code : "",
                    ZoneStatus = user.ZoneObj != null ? user.ZoneObj.Status.ToString() : "",
                    IsClientContacted = user.ISPClientContactObj != null && user.ISPClientContactObj.IsContacted,
                    Ordered = user.OrdersObj != null && user.OrdersObj.Any(o => o.Status != OrderStatus.Canceled) ? "Yes" : "No",
                    ShowCell = user.ClientContactMethods != null && user.ClientContactMethods.Any(c => c.ContactMethodId == 1),
                    ShowEmail = user.ClientContactMethods != null && user.ClientContactMethods.Any(c => c.ContactMethodId == 2),
                    ShowLandline = user.ClientContactMethods != null && user.ClientContactMethods.Any(c => c.ContactMethodId == 3),
                    IsSMS = user.ClientContactMethods != null && user.ClientContactMethods.Any(c => c.ContactMethodId == 4) ? "Yes" : "No",
                });
        }
    }
}