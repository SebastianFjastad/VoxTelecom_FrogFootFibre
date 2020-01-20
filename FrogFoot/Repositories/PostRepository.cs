using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrogFoot.Context;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Utilities;

namespace FrogFoot.Repositories
{
    public class PostRepository
    {
        private ApplicationDbContext db = Db.GetInstance();

        public IQueryable<Post> GetPosts()
        {
            return db.Posts
                .Include(p => p.Location)
                .Include(p => p.PostImage)
                .Include(p => p.Zone)
                .Where(p => !p.IsDeleted);
        }

        public List<Post> GetPostsForClient(string userId, PostType? type, int[] postIdsToExclude, int pageNo = 1, int pageSize = 5)
        {
            var posts = new List<Post>();
            var precinctCode = "";

            if (!string.IsNullOrEmpty(userId)) //get posts for logged in users
            {
                var user = db.Users.Include(u => u.Location).First(u => u.Id == userId);
                if (user.Location != null)
                {
                    precinctCode = user.Location.PrecinctCode;
                }

                if (postIdsToExclude == null)
                {
                    postIdsToExclude = new int[0];
                }

                posts = (from p in db.Posts
                    .Include(p => p.PostImage)
                    .Include(p => p.Zone)
                         where !p.IsDeleted
                               && !postIdsToExclude.Contains(p.PostId)
                               && p.PublishDate < DateTime.Now
                               && (type == null || p.Type == type)
                               && (p.GridType == PostGridding.Public
                                   || p.GridType == PostGridding.Clients //for clients gridding
                                   || (p.GridType == PostGridding.Zone && p.ZoneId != null && p.ZoneId == user.ZoneId) //for zone gridding
                                   || (p.GridType == PostGridding.Precinct && (p.PrecinctCode == null || p.PrecinctCode == "") && p.LocationId != null && p.LocationId == user.LocationId) //Suburb only gridding
                                   || (p.GridType == PostGridding.Precinct && p.PrecinctCode != null && p.PrecinctCode != "" && p.PrecinctCode == precinctCode)) //whole precinct gridding 
                         select p)
                    .OrderByDescending(p => p.PublishDate)
                    .Skip(((pageNo) - 1) * pageSize)
                    .Take(pageSize).ToList();
            }
            else  //get posts for public
            {
                posts = (from p in db.Posts
                   .Include(p => p.PostImage)
                         where !p.IsDeleted
                               && p.PublishDate < DateTime.Now
                               && p.GridType == PostGridding.Public
                         select p)
                   .OrderByDescending(p => p.PublishDate)
                   .Skip(((pageNo) - 1) * pageSize)
                   .Take(pageSize).ToList();
            }

            return posts;
        }

        public List<Post> GetPostsForChamp(string userId)
        {
            var user = db.Users.Include(u => u.Zone).Include(u => u.Estate).Include(u => u.Location).FirstOrDefault(u => u.Id == userId);

            var posts = (from p in db.Posts
                .Include(p => p.Zone)
                         where !p.IsDeleted
                               && p.Type != PostType.Article
                               //if user is champ of whole precinct
                               && ((user.UsePrecinctCodeForChamp) &&
                                ((p.GridType == PostGridding.Zone && p.Zone.PrecinctCode == user.Location.PrecinctCode) //for zone gridding
                                   || (p.GridType == PostGridding.Precinct && (p.PrecinctCode == null || p.PrecinctCode == "") && p.LocationId != null && p.LocationId == user.LocationId) //Suburb only gridding
                                   || (p.GridType == PostGridding.Precinct && p.PrecinctCode != null && p.PrecinctCode != "" && p.PrecinctCode == user.Location.PrecinctCode))) //whole precinct gridding 
                               
                                   //if user is only champ of zone
                                   || (p.GridType == PostGridding.Zone && p.ZoneId != null && p.ZoneId == user.ZoneId)
                               && p.CreatedByRole == "Champ"
                         select p)
                .OrderByDescending(p => p.PublishDate).ToList();
            return posts;
        }

        public Post GetPost(int postId)
        {
            return db.Posts.Include(p => p.PostImage).SingleOrDefault(p => p.PostId == postId);
        }

        public List<Location> GetLocationsByPrecinctCodeForChamp(string userId)
        {
            var user = db.Users.Include(u => u.Location).FirstOrDefault(u => u.Id == userId);
            var locations = db.Locations.Where(l => l.PrecinctCode == user.Location.PrecinctCode && l.PrecinctCode != null).ToList();
            return locations;
        }

        public List<Zone> GetZonesByPrecinctCodeForChamp(string userId)
        {
            var zones = new List<Zone>();

            var user = db.Users
                .Include(u => u.Location)
                .Include(u => u.Estate)
                .FirstOrDefault(u => u.Id == userId);

            //if user is in estate and able to post to the Precinct
            //get all zones in all locations in precinct
            if (user.UsePrecinctCodeForChamp || user.Estate == null)
            {
                zones.AddRange(db.Zones.Where(z => z.PrecinctCode == user.Location.PrecinctCode));
            }
            else //only get champ's zone he is in 
            {
                zones.Add(db.Zones.FirstOrDefault(z => z.ZoneId == user.ZoneId));
            }
            return zones;
        }

        public void Create(Post post, HttpPostedFileBase upload, string precinctCode, string createdBy)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                var image = new Asset
                {
                    AssetPath = Guid.NewGuid() + Path.GetFileName(upload.FileName),
                    CreatedDate = DateTime.Now
                };

                string targetFolder = HttpContext.Current.Server.MapPath("~/Assets/PostImage/");
                string targetPath = Path.Combine(targetFolder, image.AssetPath);
                upload.SaveAs(targetPath);
                post.PostImage = image;
            }

            if (post.IsEmail)
            {
                var userRepo = new UserRepository();
                IEnumerable<User> users = new List<User>();

                if (!string.IsNullOrEmpty(precinctCode)) //if the post is for precinct, get users by PrecinctCode
                {
                    users = userRepo.GetUsersByGridding(precinctCode, post.LocationId, post.ZoneId, null);
                }
                else //get users based on Location or Zone
                {
                    users = userRepo.GetUsersByGridding(null, post.LocationId, post.ZoneId, null);
                }

                //filter out unsubscribed users
                users = users.Where(u => u.FFCommsOptOutStatus == false).ToList();

                Parallel.ForEach(users,
                    user =>
                    {
                        var email = new EmailDto
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Subject = post.Title,
                            Body = post.Body
                        };

                        EmailSender.SendEmail(email, user);
                    });
            }

            post.CreatedDate = DateTime.Now;
            post.CreatedByRole = createdBy;
            post.PublishDate = post.PublishDate == null ? post.CreatedDate : post.PublishDate;

            db.Posts.Add(post);
            SaveChanges(db);
        }

        public void Edit(Post post, HttpPostedFileBase upload)
        {
            var postToUpdate = db.Posts.Include(p => p.PostImage).SingleOrDefault(p => p.PostId == post.PostId);

            if (upload != null && upload.ContentLength > 0)
            {
                if (postToUpdate != null)
                {
                    if (postToUpdate.PostImage != null)
                    {
                        //delete the file from Assets folder
                        string path = HttpContext.Current.Server.MapPath("~/Assets/PostImage/" + postToUpdate.PostImage.AssetPath);
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }

                        //delete the file path object from the DB
                        db.Assets.Remove(postToUpdate.PostImage);
                        db.SaveChanges();
                    }

                    var image = new Asset
                    {
                        AssetPath = Guid.NewGuid() + Path.GetFileName(upload.FileName),
                    };

                    string targetFolder = HttpContext.Current.Server.MapPath("~/Assets/PostImage/");
                    string targetPath = Path.Combine(targetFolder, image.AssetPath);
                    upload.SaveAs(targetPath);

                    postToUpdate.PostImage = image;
                }
            }

            postToUpdate.PrecinctCode = post.PrecinctCode;
            postToUpdate.LocationId = post.LocationId;
            postToUpdate.ZoneId = post.ZoneId;
            postToUpdate.PublishDate = post.PublishDate;
            postToUpdate.Title = post.Title;
            postToUpdate.Type = post.Type;
            postToUpdate.GridType = post.GridType;
            postToUpdate.Body = post.Body;

            db.SaveChanges();
        }

        public void Delete(int postId)
        {
            var postToDelete = db.Posts.First(p => p.PostId == postId);
            postToDelete.IsDeleted = true;
            db.SaveChanges();
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