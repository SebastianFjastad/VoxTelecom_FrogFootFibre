using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using FrogFoot.Areas.ISPAdmin.Models;
using FrogFoot.Context;
using FrogFoot.Entities;

namespace FrogFoot.Repositories
{
    public class ISPRepository
    {
        private ApplicationDbContext db = Db.GetInstance();

        #region ISPs
        public IQueryable<ISP> GetISPs()
        {
            return db.ISPs.Where(i => !i.IsDeleted);
        }

        public bool SaveISP(ISP isp)
        {
            ISP entity = isp;

            if (isp.ISPId == null)
            {
                db.ISPs.Add(entity);
            }
            else
            {
                db.ISPs.Attach(entity);
                db.Entry(entity).State = EntityState.Modified;
            }

            db.SaveChanges();
            return true;
        }

        public bool DeleteISP(int ispId)
        {
            var isp = db.ISPs.Find(ispId);
            isp.IsDeleted = true;
            db.SaveChanges();
            return false;
        }

        public ISP GetISP(int ispId)
        {
            return db.ISPs.FirstOrDefault(i => i.ISPId == ispId);
        }

        public List<ISPDto> GetISPsForProducts()
        {
            var isps = db.ISPs.Where(i => !i.IsDeleted).Select(i => new ISPDto
            {
                ISPId = i.ISPId,
                Name = i.Name,
                EmailAddress = i.EmailAddress,
                ProdCount = i.ISPProducts.Count(p => !p.IsDeleted)
            }).ToList();

            return isps;
        }
        #endregion

        #region ISP Products

        public ISPProduct GetProduct(int prodId)
        {
            return db.ISPProducts
                .Include(p => p.ISP)
                .Include(p => p.ISPLogo)
                .FirstOrDefault(p => p.ISPProductId == prodId && !p.IsDeleted);
        }

        public IQueryable<ISPProduct> GetProducts()
        {
            return db.ISPProducts.Where(p => !p.IsDeleted);
        } 

        public List<ISPProduct> GetProductsForClient(string userId)
        {
            var user = db.Users.Find(userId);

            var products = db.ISPProducts
                .Include(i => i.ISPLogo)
                .Include(p => p.ISP)
                .Where(p => !p.IsDeleted && p.IsActive &&
                            ((user.EstateId != null && p.ISPEstateProducts.Any(x => x.EstateId == user.EstateId))
                             || (user.EstateId == null && p.ISPLocationProducts.Any(x => x.LocationId == user.LocationId))))
                .ToList();

            return products;
        }

        public List<int> GetGriddedProductIds(int? locId, int? estateId, int? ispId, bool? capped, bool? isM2MClientContract)
        {
            var productIds = db.ISPProducts
               .Where(p => !p.IsDeleted && p.IsActive);
            if (isM2MClientContract != null)
            {
                productIds = isM2MClientContract == true ? productIds.Where(p => p.IsM2MClient) : productIds.Where(p => p.Is24MClient);
            }
            if (capped != null)
                productIds = productIds.Where(p => capped == p.IsCapped);
            if (ispId != null)
                productIds = productIds.Where(p => p.ISPId == ispId);
            if (estateId != null)
                productIds = productIds.Where(p => p.ISPEstateProducts.Any(x => x.EstateId == estateId));
            else if (locId != null)
                productIds = productIds.Where(p => p.ISPLocationProducts.Any(x => x.LocationId == locId));
            else
                productIds = productIds.Where(p => p.ISPEstateProducts.Any() || p.ISPLocationProducts.Any());
            return productIds.Select(p => p.ISPProductId).ToList();
        }

        public List<ISPProduct> GetProductsForPublic(string precinctCode)
        {
            var products = db.ISPProducts
                .Include(i => i.ISPLogo)
                .Include(p => p.ISP)
                .Where(p => !p.IsDeleted && p.IsActive && (p.ISPEstateProducts.Any() || p.ISPLocationProducts.Any()));

            if (!string.IsNullOrEmpty(precinctCode))
            {
                products = products.Where(p => p.ISPLocationProducts.Any(lp => lp.Location.PrecinctCode == precinctCode));
            }

            return products.ToList();
        }

        public List<ISPProduct> GetProductsForISP(int ispId)
        {
            return db.ISPProducts
                .Include(i => i.ISPLogo)
                .Where(i => i.ISPId == ispId && !i.IsDeleted).ToList();
        }

        public List<ISPProduct> GetProductsByISPUserId(string userId, int? ispId)
        {
            var _ispId = ispId;

            if (ispId == null)
            {
                _ispId = db.Users.Find(userId).ISPId;
            }

            return db.ISPProducts
                .Include(i => i.ISPLogo)
                .Include(p => p.Orders)
                .Where(i => i.ISPId == _ispId && !i.IsDeleted).ToList();
        }

        public bool SaveProduct(ISPProduct product, HttpPostedFileBase upload)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                var image = new Asset
                {
                    AssetPath = Guid.NewGuid() + Path.GetFileName(upload.FileName),
                };

                string targetFolder = HttpContext.Current.Server.MapPath("~/Assets/ISPProductLogo/");
                string targetPath = Path.Combine(targetFolder, image.AssetPath);
                upload.SaveAs(targetPath);
                product.ISPLogo = image;
            }
            product.IsActive = false;
            db.ISPProducts.Add(product);
            db.SaveChanges();
            return false;
        }

        public bool EditProduct(ISPProduct product, HttpPostedFileBase upload)
        {
            var prodToUpdate = db.ISPProducts.Include(p => p.ISPLogo).SingleOrDefault(p => p.ISPProductId == product.ISPProductId);

            if (upload != null && upload.ContentLength > 0)
            {
                if (prodToUpdate != null)
                {
                    if (prodToUpdate.ISPLogo != null)
                    {
                        //delete the file from Assets folder
                        string path = HttpContext.Current.Server.MapPath("~/Assets/ISPProductLogo/" + prodToUpdate.ISPLogo.AssetPath);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }

                        //delete the file path object from the DB
                        db.Assets.Remove(prodToUpdate.ISPLogo);
                        db.SaveChanges();
                    }

                    var image = new Asset
                    {
                        AssetPath = Guid.NewGuid() + Path.GetFileName(upload.FileName),
                    };

                    string targetFolder = HttpContext.Current.Server.MapPath("~/Assets/ISPProductLogo/");
                    string targetPath = Path.Combine(targetFolder, image.AssetPath);
                    upload.SaveAs(targetPath);

                    prodToUpdate.ISPLogo = image;
                }
            }

            prodToUpdate.LineSpeed = product.LineSpeed;
            prodToUpdate.Description = product.Description;
            prodToUpdate.ProductName = product.ProductName;
            prodToUpdate.IsCapped = product.IsCapped;
            prodToUpdate.Cap = product.Cap;
            prodToUpdate.UpSpeed = product.UpSpeed;
            prodToUpdate.IsSpecial = product.IsSpecial;

            prodToUpdate.OnceOfFFPaymentForM2M = product.OnceOfFFPaymentForM2M;
            prodToUpdate.IsM2MClient = product.IsM2MClient;
            prodToUpdate.M2MMonthlyCost = product.M2MMonthlyCost;
            prodToUpdate.Is24MClient = product.Is24MClient;
            prodToUpdate.MonthlyCost = product.MonthlyCost;
            prodToUpdate.SetupCost = product.SetupCost;
            prodToUpdate.M2MSetupCost = product.M2MSetupCost;
            prodToUpdate.IsM2MFrogfootLink = product.IsM2MFrogfootLink;

            prodToUpdate.Attr1 = product.Attr1;
            prodToUpdate.Attr2 = product.Attr2;
            prodToUpdate.Attr3 = product.Attr3;
            prodToUpdate.Attr4 = product.Attr4;

            prodToUpdate.Info1Heading = product.Info1Heading;
            prodToUpdate.Info2Heading = product.Info2Heading;
            prodToUpdate.Info3Heading = product.Info3Heading;
            prodToUpdate.Info4Heading = product.Info4Heading;

            prodToUpdate.Info1 = product.Info1;
            prodToUpdate.Info2 = product.Info2;
            prodToUpdate.Info3 = product.Info3;
            prodToUpdate.Info4 = product.Info4;

            prodToUpdate.Shaped = product.Shaped;
            prodToUpdate.Router = product.Router;
            prodToUpdate.Phone = product.Phone;
            prodToUpdate.Install = product.Install;
            prodToUpdate.Video = product.Video;
            prodToUpdate.MobileData = product.MobileData;
            prodToUpdate.CCTV = product.CCTV;

            db.SaveChanges();
            return true;
        }

        public void SaveProductsStatus(List<ProductActiveDto> products)
        {
            foreach (var prod in products)
            {
                var p = db.ISPProducts.Find(prod.prodId);
                p.IsActive = prod.isActive;
                db.SaveChanges();
            }

        }

        public void DeleteProduct(int prodId)
        {
            var product = db.ISPProducts.Find(prodId);
            product.IsDeleted = true;
            db.SaveChanges();
        }

        public void ISPDeleteProduct(string userId, int prodId, int ispId)
        {
            var user = db.Users.Find(userId);
            if (user.IsUserTypeAdmin && user.ISPId == ispId)
            {
                var prodToDelete = db.ISPProducts.Find(prodId);
                prodToDelete.IsDeleted = true;
                db.SaveChanges();
            }
        }

        #endregion

        #region Discounts/Specials
        public List<ISPEstateDiscount> GetEstateDiscounts(string userId)
        {
            var ispId = db.Users.First(u => u.Id == userId).ISPId;
            return db.ISPEstateDiscounts.Where(d => d.ISPId == ispId).ToList();
        }

        public List<Special> GetSpecials()
        {
            return db.Specials.Where(s => s.IsDeleted == false).ToList();

        }
        #endregion

        #region Client Contact
        public void CreateISPClientContact(string userId, string clientId, int ispId)
        {
            db.ISPClientContacts.Add(new ISPClientContact
            {
                ClientId = clientId,
                ISPUserId = userId,
                ISPId = ispId,
                ContactCreatedDate = DateTime.Now
            });
            db.SaveChanges();
        }

        public void MakeContact(string clientId, int ispId)
        {
            var contact = db.ISPClientContacts.FirstOrDefault(c => c.ClientId == clientId && c.ISPId == ispId);
            if (contact != null)
            {
                contact.ContactMadeDate = DateTime.Now;
                contact.IsContacted = true;
                db.SaveChanges();
            }
        }
        #endregion
    }

    public class ISPDto
    {
        public int? ISPId { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public int ProdCount { get; set; }
    }
}
