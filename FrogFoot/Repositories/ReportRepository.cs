using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FrogFoot.Areas.Admin.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Models;
using FrogFoot.Models.Reports;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FrogFoot.Repositories
{
    public class ReportRepository
    {
        private ApplicationDbContext db = Db.GetInstance();

        public List<ReportDataDto> GetAdminReports(int? locationId, DateTime? frm, DateTime? to)
        {

            var orders = (from u in db.Users.Include(u => u.Zone)
                          from o in db.Orders
                          from p in db.ISPProducts
                          from i in db.ISPs
                          from l in db.Locations
                          from ffp in db.FFProducts

                          where u.Id == o.ClientId
                                && p.ISPProductId == o.ISPProductId
                                && i.ISPId == o.ISPId
                                && l.LocationId == u.LocationId
                                && p.LineSpeed == ffp.LineSpeed
                                && (locationId == null || u.LocationId == locationId)
                                && (frm == null || o.CreatedDate > frm)
                                && (to == null || o.CreatedDate < to)
                          select new ReportDataDto
                          {
                              CreatedDate = o.CreatedDate.ToString(),
                              FirstName = u.FirstName,
                              LastName = u.LastName,
                              Email = u.Email,
                              PhoneNumber = u.PhoneNumber,
                              Landline = u.Landline,
                              OrderId = o.OrderId,
                              Status = o.Status,
                              ProductName = p.ProductName,
                              LineSpeed = p.LineSpeed,
                              FFSetupCost = ffp.UnitPriceOnceOff,
                              FFMonthlyRevenue = ffp.UnitPriceMonthly,
                              UpSpeed = p.UpSpeed,
                              IsCapped = p.IsCapped ? "Yes" : "No",
                              IsOwner = u.IsOwner ? "Yes" : "No",
                              CreatedByRole = o.CreatedByRole,
                              MonthlyCost = o.ClientContractTerm == ContractTerm.Month24 ? p.MonthlyCost : p.M2MMonthlyCost,
                              SetupCost = o.ClientContractTerm == ContractTerm.Month24 ? p.SetupCost : p.M2MSetupCost,
                              ISPName = i.Name,
                              Precinct = l.Name,
                              Zone = u.Zone != null ? u.Zone.Code : "none",
                              Address = u.Address,
                              FirstDateOfFibre = u.Zone != null ? u.Zone.FirstDateOfFibre.ToString() : "none",
                              StatusList = o.StatusList.ToList()
                          });
            return orders.ToList();
        }

        public ReportViewModel GetSalesReport()
        {
            var model = new ReportViewModel
            {
                OrdersByLocation = GetOrdersByLocation(),
                OrdersByFFProduct = GetOrdersByFFProduct(),
                OrdersByMonth = GetOrdersByMonth(),
                OrdersByISP = GetOrdersByISP(),
                AllOrders = GetAllOrders(),
                UsersWithOrders = GetAllUsersWithOrders(),
                Reports = GetAdminReports(null, null, null),
                Locations = db.Locations.Where(l => !l.IsDeleted).ToList()
            };

            //calculate the percentage of product types make up of total orders
            var allOrders = model.OrdersByFFProduct.Sum(x => x.CountAll);
            var voxOrders = model.OrdersByFFProduct.Sum(x => x.CountVox);
            foreach (var prod in model.OrdersByFFProduct)
            {
                prod.FFProdOrderPerc = allOrders != 0 ? (decimal)prod.CountAll / allOrders * 100 : 0;
                prod.VoxProdOrderPerc = voxOrders != 0 ? (decimal)prod.CountVox / voxOrders * 100 : 0;
            }

            //build up the Monthly overview list
            decimal? residentTotal = model.OrdersByLocation.Sum(x => x.Residents);
            int orderCount = 0;
            int voxOrderCount = 0;
            decimal? ffRevenue = 0;
            decimal? voxRevenue = 0;
            decimal? groupRevenue = 0;
            decimal? ffARPU = 0;
            decimal? groupARPU = 0;

            for (int i = 0; i < model.OrdersByMonth.Count; i++)
            {
                var orderMonthGroup = model.OrdersByMonth[i];
                orderCount += orderMonthGroup.CountAll;
                voxOrderCount += orderMonthGroup.CountVox;
                ffRevenue += orderMonthGroup.FFValue;
                voxRevenue += orderMonthGroup.VoxValue;
                groupRevenue += orderMonthGroup.GroupValue;
                ffARPU = orderCount != 0 ? ffRevenue/orderCount : 0;
                groupARPU = voxOrderCount != 0 ? ((ffRevenue / orderCount) + (voxRevenue / voxOrderCount))/2 : (ffRevenue / orderCount);
                     
                model.MonthlyOverview.Add(new MonthlyOverview
                {
                    Month = "Month " + (i + 1),
                    Penetration = residentTotal != 0 ? (orderCount / residentTotal * 100) : 0,
                    Orders = orderCount,
                    FFRevenue = ffRevenue,
                    VoxRevenue = voxRevenue,
                    GroupRevenue = groupRevenue,
                    FFARPU = ffARPU,
                    GroupARPU = groupARPU
                });
            }

            //build up the ARPU list
            for (int i = 0; i < model.OrdersByFFProduct.Count; i++)
            {
                var orderProductGroup = model.OrdersByFFProduct[i];
                var ffValue = orderProductGroup.CountAll != 0 ? orderProductGroup.FFValue / orderProductGroup.CountAll : 0;
                var voxValue = orderProductGroup.CountVox != 0 ? (orderProductGroup.VoxValue / orderProductGroup.CountVox - ffValue) : 0;
                model.ARPU.Add(new ARPU
                {
                    FFProduct = (LineSpeed)orderProductGroup.LineSpeed,
                    Frogfoot = ffValue,
                    Vox = voxValue,
                    Group = ffValue + voxValue * (orderProductGroup.CountVox != 0 && orderProductGroup.CountAll != 0 ? orderProductGroup.CountVox / (decimal)orderProductGroup.CountAll : 1)
                });
            }

            var groupCountLoc = model.OrdersByLocation.Count;
            var countAll = model.OrdersByLocation.Sum(x => x.CountAll);

            //work out the column totals and/or averages
            model.SumVoxOrderCount = model.OrdersByLocation.Sum(x => x.CountVox);
            model.SumAllOrderCount = countAll;
            model.SumFrogfootValue = model.OrdersByLocation.Sum(x => x.FFValue);
            model.SumVoxValue = model.OrdersByLocation.Sum(x => x.VoxValue);
            model.SumOtherValue = model.OrdersByLocation.Sum(x => x.OtherValue);
            model.SumGroupValue = model.OrdersByLocation.Sum(x => x.GroupValue);
            model.AvgVoxPercOrders = model.SumAllOrderCount != 0 ? (decimal)model.SumVoxOrderCount / model.SumAllOrderCount * 100 : 0;
            model.AvgVoxPercRevenue = model.SumGroupValue != 0 ? model.SumVoxValue / model.SumGroupValue * 100 : 0;
            model.AvgPercPenetration = groupCountLoc != 0 ? model.OrdersByLocation.Sum(x => x.Penetration) / groupCountLoc : 0;
            model.SumResidents = model.OrdersByLocation.Sum(x => x.Residents);

            model.AvgPercTotalOrders = model.OrdersByFFProduct.Sum(x => x.FFProdOrderPerc);
            model.AvgVoxPercTotalOrders = model.OrdersByFFProduct.Sum(x => x.VoxProdOrderPerc);

            model.AvgFFARPU = model.OrdersByLocation.Average(x => x.FFARPU);
            model.AvgGroupARPU = model.OrdersByLocation.Average(x => x.GroupARPU);

            model.AvgCappedPrice = model.OrdersByFFProduct.Average(x => x.AverageCappedPrice);
            model.AvgUncappedPrice = model.OrdersByFFProduct.Average(x => x.AverageUncappedPrice);
            model.AvgPrice = model.OrdersByFFProduct.Average(x => x.AveragePrice);
            return model;
        }

        public List<OrderData> GetOrdersByLocation()
        {
            var table1 = db.Database.SqlQuery<OrderData>("select " +
                                                      "min(l.Name) as Name, " +
                                                      "sum(case o.ISPId when 2 then 1 else 0 end) as CountVox, " +
                                                      "count(*) as CountAll, " +
                                                      "sum(ffp.UnitPriceMonthly) as FFValue, " +
                                                      "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end) as VoxValue, " +
                                                      "sum(case o.ISPId when 2 then 0 else p.MonthlyCost end) as OtherValue, " +
                                                      "sum(case o.ISPId when 2 then p.MonthlyCost + ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) as GroupValue, " +
                                                      "cast(sum(case o.ISPId when 2 then 1 else 0 end) as decimal(18,2))/cast(count(*) as decimal(18,2))*100 as VoxPercOfOrders, " +
                                                      "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end)/sum(case o.ISPId when 2 then p.MonthlyCost+ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) * 100.0 as VoxPercOfRevenue, " +
                                                      "case when min(l.Residents) = 0 then 0 else count (distinct o.ClientId)/(min(l.Residents) /100.0) end as Penetration, " +
                                                      "avg(case when o.ISPId = 2 then ffp.UnitPriceMonthly + p.MonthlyCost else ffp.UnitPriceMonthly end) as GroupARPU, " +
                                                      "avg(ffp.UnitPriceMonthly) as FFARPU, " +
                                                      "min(l.Residents) as Residents, " +
                                                      "min(u.LocationId) as LocationId " +
                                                      "from AspNetUsers u " +
                                                      "inner join [Order] o on u.id = o.ClientId " +
                                                      "inner join ISPProduct p on o.ISPProductId = p.ISPProductId " +
                                                      "inner join FFProduct ffp on p.LineSpeed = ffp.LineSpeed " +
                                                      "inner join Location l on u.LocationId = l.LocationId " +
                                                      "where o.[Status] != 5 and l.IsDeleted = 0 and u.IsDeleted = 0 " +
                                                      "group by l.LocationId").ToList();
            return table1;
        }

        public List<OrderData> GetOrdersByFFProduct()
        {
            var ordersByProduct = new List<OrderData>();

            ordersByProduct.AddRange(db.Database.SqlQuery<OrderData>("select " +
                                                     "min(ffp.Linespeed) as Linespeed, " +
                                                     "sum(case o.ISPId when 2 then 1 else 0 end) as CountVox, " +
                                                     "count(*) as CountAll, " +
                                                     "sum(ffp.UnitPriceMonthly) as FFValue, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end) as VoxValue, " +
                                                     "sum(case o.ISPId when 2 then 0 else p.MonthlyCost end) as OtherValue, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost + ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) as GroupValue, " +
                                                     "avg(case when p.IsCapped = 1 then p.MonthlyCost else null end) as AverageCappedPrice, " +
                                                     "avg(case when p.IsCapped = 0 then p.MonthlyCost else null end) as AverageUncappedPrice, " +
                                                     "avg(p.MonthlyCost) as AveragePrice, " +
                                                     "cast(sum(case o.ISPId when 2 then 1 else 0 end) as decimal(18,2))/cast(count(*) as decimal(18,2))*100 as VoxPercOfOrders, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end)/sum(case o.ISPId when 2 then p.MonthlyCost+ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) * 100.0 as VoxPercOfRevenue, " +
                                                     "case when min(l.Residents) = 0 then 0 else count (distinct o.ClientId)/(min(l.Residents) /100.0) end as Penetration, " +
                                                     "convert(bit, min(case when p.IsM2MFrogfootLink = 0 then 0 end)) as IsM2MFrogfootLink, " +
                                                     "min(l.Residents) as Residents " +
                                                     "from AspNetUsers u " +
                                                     "inner join [Order] o on u.id = o.ClientId " +
                                                     "inner join ISPProduct p on o.ISPProductId = p.ISPProductId " +
                                                     "inner join FFProduct ffp on p.LineSpeed = ffp.LineSpeed " +
                                                     "inner join Location l on u.LocationId = l.LocationId " +
                                                     "where o.[Status] != 5 and l.IsDeleted = 0 and u.IsDeleted = 0 and p.IsM2MFrogfootLink = 0" +
                                                     "group by ffp.Linespeed").ToList());

            ordersByProduct.AddRange(db.Database.SqlQuery<OrderData>("select " +
                                                     "min(ffp.Linespeed) as Linespeed, " +
                                                     "sum(case o.ISPId when 2 then 1 else 0 end) as CountVox, " +
                                                     "count(*) as CountAll, " +
                                                     "sum(ffp.UnitPriceMonthly) as FFValue, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end) as VoxValue, " +
                                                     "sum(case o.ISPId when 2 then 0 else p.MonthlyCost end) as OtherValue, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost + ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) as GroupValue, " +
                                                     "avg(case when p.IsCapped = 1 then p.MonthlyCost else null end) as AverageCappedPrice, " +
                                                     "avg(case when p.IsCapped = 0 then p.MonthlyCost else null end) as AverageUncappedPrice, " +
                                                     "avg(p.MonthlyCost) as AveragePrice, " +
                                                     "cast(sum(case o.ISPId when 2 then 1 else 0 end) as decimal(18,2))/cast(count(*) as decimal(18,2))*100 as VoxPercOfOrders, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end)/sum(case o.ISPId when 2 then p.MonthlyCost+ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) * 100.0 as VoxPercOfRevenue, " +
                                                     "case when min(l.Residents) = 0 then 0 else count (distinct o.ClientId)/(min(l.Residents) /100.0) end as Penetration, " +
                                                     "convert(bit, min(case when p.IsM2MFrogfootLink=1 then 1 end)) as IsM2MFrogfootLink, " +
                                                     "min(l.Residents) as Residents " +
                                                     "from AspNetUsers u " +
                                                     "inner join [Order] o on u.id = o.ClientId " +
                                                     "inner join ISPProduct p on o.ISPProductId = p.ISPProductId " +
                                                     "inner join FFProduct ffp on p.LineSpeed = ffp.LineSpeed " +
                                                     "inner join Location l on u.LocationId = l.LocationId " +
                                                     "where o.[Status] != 5 and l.IsDeleted = 0 and u.IsDeleted = 0 and p.IsM2MFrogfootLink = 1" +
                                                     "group by ffp.Linespeed").ToList());
            return ordersByProduct;
        }

        public List<OrderData> GetOrdersByMonth()
        {
            return db.Database.SqlQuery<OrderData>("select " +
                                                     "convert(NVARCHAR(7), o.CreatedDate, 120) as Name, " +
                                                     "min(o.CreatedDate) as MonthPeriod, " +
                                                     "sum(case o.ISPId when 2 then 1 else 0 end) as CountVox, " +
                                                     "count(*) as CountAll, " +
                                                     "sum(ffp.UnitPriceMonthly) as FFValue, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end) as VoxValue, " +
                                                     "sum(case o.ISPId when 2 then 0 else p.MonthlyCost end) as OtherValue, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost + ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) as GroupValue, " +
                                                     "cast(sum(case o.ISPId when 2 then 1 else 0 end) as decimal(18,2))/cast(count(*) as decimal(18,2))*100 as VoxPercOfOrders, " +
                                                     "sum(case o.ISPId when 2 then p.MonthlyCost else 0 end)/sum(case o.ISPId when 2 then p.MonthlyCost+ffp.UnitPriceMonthly else ffp.UnitPriceMonthly end) * 100.0 as VoxPercOfRevenue, " +
                                                     "case when min(l.Residents) = 0 then 0 else count (distinct o.ClientId)/(min(l.Residents) /100.0) end as Penetration, " +
                                                     "avg(case when o.ISPId = 2 then ffp.UnitPriceMonthly + p.MonthlyCost else ffp.UnitPriceMonthly end) as GroupARPU, " +
                                                     "avg(ffp.UnitPriceMonthly) as FFARPU, " +
                                                     "min(l.Residents) as Residents " +
                                                     "from AspNetUsers u " +
                                                     "left join [Order] o on u.id = o.ClientId " +
                                                     "inner join ISPProduct p on o.ISPProductId = p.ISPProductId " +
                                                     "inner join FFProduct ffp on p.LineSpeed = ffp.LineSpeed " +
                                                     "inner join Location l on u.LocationId = l.LocationId " +
                                                     "where o.[Status] != 5 and l.IsDeleted = 0 and u.IsDeleted = 0 " +
                                                     "group by convert(NVARCHAR(7), o.CreatedDate, 120)").ToList();
        }

        public List<OrderData> GetAllOrders()
        {
            return db.Database.SqlQuery<OrderData>("select " +
                                                    "ffp.Linespeed as Linespeed, " +
                                                    "o.CreatedDate as CreatedDate, " +
                                                    "u.LocationId as LocationId, " +
                                                    "ffp.UnitPriceMonthly as FFMonthlyRevenue, " +
                                                    "ffp.UnitPriceOnceOff as FFSetupRevenue, " +
                                                    "p.MonthlyCost as ISPMonthlyRevenue, " +
                                                    "p.SetupCost as ISPSetupRevenue, " +
                                                    "p.ISPId as ISPId " +
                                                    "from AspNetUsers u " +
                                                    "left join [Order] o on o.ClientId = u.Id " +
                                                    "left join ISPProduct p on o.ISPProductId = p.ISPProductId " +
                                                    "left join FFProduct ffp on p.LineSpeed = ffp.LineSpeed " +
                                                    "where o.[Status] != 5 and u.IsDeleted = 0"
                                                    ).ToList();
        }

        public List<UserInterestData> GetAllUsersWithOrders()
        {
            return db.Database.SqlQuery<UserInterestData>("select " +
                                                    "o.CreatedDate as CreatedDate, " +
                                                    "u.CreatedDate as RegisteredDate, " +
                                                    "UserHasOrder = convert(bit, case when o.OrderId is not null then 1 else 0 end), " +
                                                    "p.ISPId as ISPId, " +
                                                    "u.ZoneId as ZoneId, " +
                                                    "u.LocationId as LocationId, " +
                                                    "l.PrecinctCode as PrecinctCode " +
                                                    "from AspNetUsers u " +
                                                    "left join [Order] o on o.ClientId = u.Id " +
                                                    "left join Location l on l.LocationId = u.LocationId " +
                                                    "left join ISPProduct p on o.ISPProductId = p.ISPProductId " +
                                                    "left join FFProduct ffp on p.LineSpeed = ffp.LineSpeed " +
                                                    "where (o.[Status] is null or o.[Status] != 5 and u.IsDeleted = 0)").ToList();
        }

        public List<ReportDataDto> GetISPReports(string userId, int? locationId, DateTime? frm, DateTime? to)
        {
            var user = db.Users.Find(userId);

            var reports = from u in db.Users.Include(u => u.Zone)
                          from o in db.Orders
                          from p in db.ISPProducts
                          from l in db.Locations

                          where u.Id == o.ClientId
                                && !u.IsDeleted
                                && p.ISPProductId == o.ISPProductId
                                && o.ISPId == user.ISPId
                                && l.LocationId == u.LocationId
                                && (locationId == null || u.LocationId == locationId)
                                && (frm == null || o.CreatedDate > frm)
                                && (to == null || o.CreatedDate < to)
                          select new ReportDataDto
                          {
                              FirstName = u.FirstName,
                              LastName = u.LastName,
                              Email = u.Email,
                              PhoneNumber = u.PhoneNumber,
                              Landline = u.Landline,
                              IsOwner = u.IsOwner ? "Yes" : "No",
                              OrderId = o.OrderId,
                              Status = o.Status,
                              ProductName = p.ProductName,
                              LineSpeed = p.LineSpeed,
                              UpSpeed = p.UpSpeed,
                              IsCapped = p.IsCapped ? "Yes" : "No",
                              CreatedByRole = o.CreatedByRole,
                              MonthlyCost = p.MonthlyCost,
                              SetupCost = p.SetupCost,
                              Precinct = l.Name,
                              Zone = u.Zone != null ? u.Zone.Code : "none"
                          };

            return reports.ToList();
        }

        public List<ReportDataDto> GetUsersReport()
        {
            var manager = new UserManager<User>(new UserStore<User>(db));

            var users = from u in db.Users
                        .Include(u => u.Estate)
                        .Include(u => u.Zone)
                        .Include(u => u.Location)
                        .Include(u => u.Orders)

                        where !u.IsDeleted

                        select new ReportDataDto
                        {
                            Id = u.Id,
                            Precinct = u.Location.PrecinctCode ?? "Other",
                            Suburb = u.Location.Name ?? "",
                            Estate = u.Estate.Name ?? "",
                            Zone = u.Zone.Code ?? "",
                            Address = u.Address,
                            Latitude = u.Latitude,
                            Longitude = u.Longitude,
                            UserCreatedBy = u.UserCreatedWithOrder ? "ISP" : "Registration",
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Email = u.Email,
                            PhoneNumber = u.PhoneNumber,
                            Landline = u.Landline,
                            CreatedDate = u.CreatedDate.ToString(),
                            OrderedOn = u.Orders.FirstOrDefault().CreatedDate.ToString(),
                            Status = u.Orders.FirstOrDefault().Status
                        };

            return users.ToList().Where(u => manager.IsInRole(u.Id, "Client")).ToList();
        }

        public List<ReportDataDto> GetISPUsersData()
        {
            return (from u in db.Users
                    .Include(u => u.ISP)
                    where u.ISPId != null
                    select new ReportDataDto
                    {
                        ISPName = u.ISP.Name,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        CreatedDate = u.CreatedDate.ToString(),
                        LogInCount = u.LogInCount,
                        LastLogInDate = u.LastLogin.ToString(),
                    }).OrderBy(u => u.ISPName).ToList();
        }

        public List<ISPReportData> GetOrdersByISP()
        {
            var ISPs = db.Orders
                .Include(o => o.ISP)
                .Include(o => o.Client)
                .Include(o => o.Client.Location)
                .Where(o => o.Status != OrderStatus.Canceled 
                && o.Client != null && !o.Client.IsDeleted
                && o.Client.Location != null 
                && !o.Client.Location.IsDeleted)
                .GroupBy(o => o.ISPId)
                .Select(o => new ISPReportData
                {
                    ISPName = o.FirstOrDefault().ISP.Name,
                    Orders = o.Count(),
                    BBValue = o.Sum(x => x.ISPProduct.MonthlyCost ?? 0),
                    LinksValue = o.Sum(x => x.OrderFFProducts.FirstOrDefault(y => y.FFProduct.Type == ProductType.LineSpeed).FFProduct.UnitPriceMonthly ?? 0),
                    StatusNew = o.Count(x => x.Status == OrderStatus.New),
                    StatusPending = o.Count(x => x.Status == OrderStatus.Pending),
                    StatusOrdered = o.Count(x => x.Status == OrderStatus.Ordered),
                    StatusAccepted = o.Count(x => x.Status == OrderStatus.Accepted),
                }).ToList();

            foreach (var isp in ISPs)
            {
                var percOrders = ISPs.Sum(i => i.Orders);
                var percRevenue = ISPs.Sum(i => i.BBValue);

                isp.ISPPercOfOrders = percOrders != 0 ? isp.Orders / (decimal)ISPs.Sum(i => i.Orders) * 100 : 0;
                isp.ISPPercOfRevenue = percRevenue != 0 ? isp.BBValue / ISPs.Sum(i => i.BBValue) * 100 : 0;
            }

            return ISPs;
        }
    }
}