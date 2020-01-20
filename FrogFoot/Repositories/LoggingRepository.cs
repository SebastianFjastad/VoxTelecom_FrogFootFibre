using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Entities;

namespace FrogFoot.Repositories
{
    public class LoggingRepository
    {
        private ApplicationDbContext db =Db.GetInstance();

        public async Task LogUserLogIn(string userId)
        {
            var user = db.Users.Find(userId);
            if (user != null)
            {
                user.LogInCount += 1;
                user.LastLogin = DateTime.Now;
                await db.SaveChangesAsync();
            }
        }
    }
}