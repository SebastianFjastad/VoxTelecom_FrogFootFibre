using System.Collections.Generic;
using System.Linq;
using FrogFoot.Areas.Client.Models;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Repositories;

namespace FrogFoot.Services
{
    public class FibreChampService
    {
        private PostRepository postRepo = new PostRepository();
        private UserRepository userRepo = new UserRepository();

        #region Posts
        public List<Post> GetPosts(string userId)
        {
            return postRepo.GetPostsForChamp(userId);
        }

        public Post GetPost(int postId)
        {
            return postRepo.GetPost(postId);
        }

        public PostViewModel GetPostCreateData(string userId)
        {
            return new PostViewModel
            {
                Locations = postRepo.GetLocationsByPrecinctCodeForChamp(userId),
                Zones = postRepo.GetZonesByPrecinctCodeForChamp(userId),
                User = userRepo.GetUser(userId)
            };
        }

        public void CreatePost(Post post, string precinctCode)
        {
            postRepo.Create(post, null, precinctCode, "Champ");
        }

        public void EditPost(Post post)
        {
            postRepo.Edit(post, null);
        }

        public void DeletePost(int postId)
        {
            postRepo.Delete(postId);
        }

        #endregion

        #region Users
        public User GetUser(string userId)
        {
            return userRepo.GetUser(userId);
        }

        public IQueryable<User> GetUsers(string userId)
        {
            return userRepo.GetUsersByGriddingForChamp(userId);
        }
        #endregion
    }
}