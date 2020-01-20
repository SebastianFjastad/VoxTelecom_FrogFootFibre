using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Repositories;

namespace FrogFoot.Services
{
    public class LoggingService
    {
        private LoggingRepository loggingRepo = new LoggingRepository();

        public async Task LogUserLogIn(string userId)
        {
            await loggingRepo.LogUserLogIn(userId);
        }
    }
}