using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc.Html;
using FrogFoot.Context;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Utilities;

namespace FrogFoot.Repositories
{
    public class GriddingRepository
    {
        private readonly ApplicationDbContext db = Db.GetInstance();

        #region Locations
        public IQueryable<Location> GetLocations()
        {
            return db.Locations.Where(l => !l.IsDeleted);
        }

        public bool SaveLocation(string name, string apiName, string code, bool active, bool allowOrder, int residents)
        {
            db.Locations.Add(new Location
            {
                Name = name,
                APIName = apiName,
                PrecinctCode = code,
                IsActive = active,
                AllowOrder = allowOrder,
                Residents = residents
            });
            db.SaveChanges();
            return false;
        }

        public bool UpdateLocation(int id, string name, string apiName, string code, bool active, bool allowOrder, int residents)
        {
            var location = db.Locations.Find(id);
            location.Name = name;
            location.APIName = apiName;
            location.PrecinctCode = code;
            location.IsActive = active;
            location.AllowOrder = allowOrder;
            location.Residents = residents;
            db.SaveChanges();
            return false;
        }

        public bool DeleteLocation(int locId)
        {
            var loc = db.Locations.Find(locId);
            loc.IsDeleted = true;
            db.SaveChanges();
            return false;
        }
        #endregion

        #region Estates
        public IQueryable<Estate> GetEstates()
        {
            return db.Estates.Where(e => !e.IsDeleted);
        }

        public IQueryable<Estate> GetEstate()
        {
            return db.Estates;
        }

        public void SaveDiscount(int? discountId, int estateId, int ispId, Discount discount)
        {
            var currDiscount = db.ISPEstateDiscounts.FirstOrDefault(d => d.ISPEstateDiscountId == discountId);
            if (currDiscount == null && discount != Discount.Zero)
            {
                db.ISPEstateDiscounts.Add(new ISPEstateDiscount
                {
                    EstateId = estateId,
                    ISPId = ispId,
                    Discount = discount
                });
            }
            else if (currDiscount != null && discount == Discount.Zero)
            {
                db.ISPEstateDiscounts.Remove(currDiscount);
            }
            else if (currDiscount != null)
            {
                currDiscount.Discount = discount;
            }

            db.SaveChanges();
        }

        public bool SaveEstate(int locationId, string name, string code)
        {
            db.Estates.Add(new Estate
            {
                LocationId = locationId,
                Name = name,
                EstateCode = code
            });
            db.SaveChanges();
            return true;
        }

        public bool UpdateEstate(int id, int locationId, string name, string code)
        {
            var estate = db.Estates.Find(id);
            estate.Name = name;
            estate.EstateCode = code;
            estate.LocationId = locationId;
            db.SaveChanges();
            return false;
        }

        public void DeleteEstate(int estateId)
        {
            var estate = db.Estates.Find(estateId);
            estate.IsDeleted = true;
            db.SaveChanges();
        }
        #endregion

        #region Zones
        public IQueryable<Zone> GetZones()
        {
            return db.Zones.Where(z => !z.IsDeleted);
        }

        public Zone GetZone(int zoneId)
        {
            return db.Zones.Find(zoneId);
        }

        public void CreateZone(Zone zone)
        {
            zone.Status = TrenchingStatus.Undefined;
            db.Zones.Add(zone);
            db.SaveChanges();
            ZoneSync.ProcessUsers(zone.PrecinctCode);
        }

        public void UpdateZone(Zone zone)
        {
            var zoneToUpdate = db.Zones.Find(zone.ZoneId);
            if (zoneToUpdate != null)
            {
                zoneToUpdate.Code = zone.Code;
                zoneToUpdate.PrecinctCode = zone.PrecinctCode;
                zoneToUpdate.Status = zone.Status;
                zoneToUpdate.AllowOrder = zone.AllowOrder;
                zoneToUpdate.AllowSpecial = zone.AllowSpecial;
                zoneToUpdate.AllowLeads = zone.AllowLeads;
                zoneToUpdate.FirstDateOfFibre = zone.FirstDateOfFibre;
                zoneToUpdate.LastDateOfFibre = zone.LastDateOfFibre;
                zoneToUpdate.TimeToInstallation = zone.TimeToInstallation;
                zoneToUpdate.NoHousesInZone = zone.NoHousesInZone;
                zoneToUpdate.NodeId = zone.NodeId;
                zoneToUpdate.NodeName = zone.NodeName;
                zoneToUpdate.NodeLatitude = zone.NodeLatitude;
                zoneToUpdate.NodeLongitude = zone.NodeLongitude;

                db.SaveChanges();
                ZoneSync.ProcessUsers(zone.PrecinctCode);
            }
        }

        public void DeleteZone(int zoneId)
        {
            var zoneToDelete = db.Zones.Find(zoneId);
            if (zoneToDelete != null)
            {
                zoneToDelete.IsDeleted = true;
                db.SaveChanges();
                ZoneSync.ProcessUsers(zoneToDelete.PrecinctCode);
            }
        }
        #endregion

        #region Gridding
        public int? UpdateProductLocationGridding(int prodId, int locId, int ispId, int? prodGridId)
        {
            var prodGridObj = db.ISPLocationProducts.Find(prodGridId);

            //if prodGridObj exists then remove entity
            if (prodGridObj != null)
            {
                db.ISPLocationProducts.Remove(prodGridObj);
                db.SaveChanges();
                return null;
            }
            //if not exists then create
            else
            {
                var prodGridToSave = new ISPLocationProduct
                {
                    ISPProductId = prodId,
                    LocationId = locId,
                    ISPId = ispId
                };
                db.ISPLocationProducts.Add(prodGridToSave);
                db.SaveChanges();
                return prodGridToSave.ISPLocationProductId;
            }
        }

        public int? UpdateProductEstateGridding(int prodId, int estId, int ispId, int? prodGridId)
        {
            var prodGridObj = db.ISPEstateProducts.Find(prodGridId);

            //if prodGridObj exists then remove entity
            if (prodGridObj != null)
            {
                db.ISPEstateProducts.Remove(prodGridObj);
                db.SaveChanges();
                return null;
            }
            //if not exists then create
            else
            {
                var prodGridToSave = new ISPEstateProduct
                {
                    ISPProductId = prodId,
                    EstateId = estId,
                    ISPId = ispId
                };
                db.ISPEstateProducts.Add(prodGridToSave);
                db.SaveChanges();
                return prodGridToSave.ISPEstateProductId;
            }
        }

        public List<PrecinctDto> GetPrecints()
        {
            var locations = db.Locations.Where(l => !l.IsDeleted && !string.IsNullOrEmpty(l.PrecinctCode));
            var estates = db.Estates.Include(e => e.Location).Where(e => !e.IsDeleted && !string.IsNullOrEmpty(e.EstateCode));

            var precincts = locations.Select(loc => new PrecinctDto
            {
                Name = loc.Name + " (" + loc.PrecinctCode + ")",
                PrecinctCode = loc.PrecinctCode,
                LocationId = loc.LocationId
            }).ToList();

            precincts.AddRange(estates.Select(estate => new PrecinctDto
            {
                Name = estate.Location.Name + " (" + estate.EstateCode + ")",
                PrecinctCode = estate.EstateCode,
                LocationId = estate.Location.LocationId
            }));

            return precincts.ToList();
        }
        #endregion
    }
}