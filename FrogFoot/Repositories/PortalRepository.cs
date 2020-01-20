using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using FrogFoot.Context;
using FrogFoot.Entities;

namespace FrogFoot.Repositories
{
    public class PortalRepository
    {
        private ApplicationDbContext db = Db.GetInstance();

        public List<Portal> GetPortals()
        {
            return db.Portals
                .Include(p => p.CoverImage)
                .Include(p => p.Urls)
                .Where(p => !p.IsDeleted).ToList();
        }

        public Portal GetPortal(int id)
        {
            return db.Portals
                .Include(p => p.CoverImage)
                .Include(p => p.Urls)
                .FirstOrDefault(p => p.PortalId == id);
        }

        public void Save(Portal portal, HttpPostedFileBase img)
        {
            var portalToUpdate = db.Portals.Include(p => p.CoverImage).FirstOrDefault(p => p.PortalId == portal.PortalId);

            //if image passed in then remove old
            if (portalToUpdate != null && img != null && img.ContentLength > 0)
            {
                if (portalToUpdate.CoverImage != null)
                {
                    //delete the file from Assets folder
                    string path =
                        HttpContext.Current.Server.MapPath("~/Assets/PortalImage/" +
                                                           portalToUpdate.CoverImage.AssetPath);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }

                    //delete the file path object from the DB
                    db.Assets.Remove(portalToUpdate.CoverImage);
                    db.SaveChanges();
                }

                var image = new Asset
                {
                    AssetPath = Guid.NewGuid() + Path.GetFileName(img.FileName),
                    CreatedDate = DateTime.Now
                };

                string targetFolder = HttpContext.Current.Server.MapPath("~/Assets/PortalImage/");
                string targetPath = Path.Combine(targetFolder, image.AssetPath);
                img.SaveAs(targetPath);
                portalToUpdate.CoverImage = image;
            }

            //if portal exists then modify
            if (portal.PortalId > 0)
            {
                var manager = ((IObjectContextAdapter)db).ObjectContext.ObjectStateManager;

                foreach (var url in portal.Urls)
                {
                    if (url.UrlId != 0)
                    {
                        db.Urls.Attach(url);
                        manager.ChangeObjectState(url, EntityState.Modified);
                    }
                    else
                    {
                        db.Urls.Add(url);
                    }
                }

                portalToUpdate.PrecinctCode = portal.PrecinctCode;
                portalToUpdate.FacebookUrl = portal.FacebookUrl;
                portalToUpdate.Name = portal.Name;
                portalToUpdate.TwitterUrl = portal.TwitterUrl;
            }
            else
            {
                var image = new Asset
                {
                    AssetPath = Guid.NewGuid() + Path.GetFileName(img.FileName),
                    CreatedDate = DateTime.Now
                };

                string targetFolder = HttpContext.Current.Server.MapPath("~/Assets/PortalImage/");
                string targetPath = Path.Combine(targetFolder, image.AssetPath);
                img.SaveAs(targetPath);
                portal.CoverImage = image;

                db.Portals.Add(portal);
            }
            db.SaveChanges();
        }

        public List<Url> GetUrls()
        {
            return db.Urls.ToList();
        }

        public void SaveUrl(Url url)
        {
            url.URL = url.URL.ToLower();

            if (url.UrlId > 0)
            {
                db.Entry(url).State = EntityState.Modified;
            }
            else
            {
                db.Urls.Add(url);
            }
            db.SaveChanges();
        }

        public void DeleteUrl(int urlId)
        {
            var url = db.Urls.Find(urlId);
            db.Urls.Remove(url);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            var portal = db.Portals.Find(id);
            portal.IsDeleted = true;
            db.SaveChanges();
        }
    }
}