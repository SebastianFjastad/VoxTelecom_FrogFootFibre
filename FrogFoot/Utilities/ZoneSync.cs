using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Entities;
using FrogFoot.Models;
using FrogFoot.Repositories;
using Hangfire;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FrogFoot.Utilities
{
    public static class ZoneSync
    {
        #region public
        public static void CheckLastModDate()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers["Content-type"] = "application/json";
                var result = client.DownloadString("http://maps.frogfoot.net/ftth/status");
                Dictionary<string, string> jsonContent = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                double unixDate = Convert.ToDouble(jsonContent["last-update"]);
                DateTime latestDate = UnixTimeStampToDateTime(unixDate);

                if (latestDate > Date)
                {
                    //process all the users
                    Date = latestDate;
                    ProcessUsers("");
                }
            }
        }

        public static void ProcessUsers(string precinctCode)
        {
            var bgClient = new BackgroundJobClient();
            bgClient.Enqueue(() => ProcessUsersTask(precinctCode));
        }

        public static void ProcessUsersTask(string precinctCode)
        {
            var db = Db.GetInstance();
            var repo = new GriddingRepository();
            var zones = repo.GetZones().ToList();
            var usersToProcess = GetUsersForZoneProcessing(precinctCode, db);
            var results = new ResponseModel { TotalRequest = usersToProcess.Count };

            foreach (var u in usersToProcess)
            {
                results.ResponseList.Add(GetStatus(u, zones));
            }
            db.SaveChanges();

            var usersWithErrorsString = "<table><thead><tr> <th>FullName</th> <th>Email</th> <th>Location</th> <th>Estate</th> <th>Lat-Long</th> <th>Address</th> <th>Message</th></tr></thead><tbody>";
            foreach (var r in results.ResponseList)
            {

                usersWithErrorsString += string.Format("<tr><td>{0} {1}</td> <td>{2}</td> <td>{3}</td> <td>{4}</td> <td>{5},{6}</td> <td>{7}</td> <td>{8}</td> </tr>", r.User.FirstName,
                    r.User.LastName, r.User.Email, (r.User.Location != null ? r.User.Location.Name : ""), (r.User.Estate != null ? r.User.Estate.Name : ""), r.User.Latitude, r.User.Longitude,
                    r.User.Address, r.ErrorMessage);
            }

            usersWithErrorsString += "</tbody>";

            var email = new EmailDto
            {
                Subject = precinctCode + " sync results",
                Body = "Precinct synced:" + (string.IsNullOrEmpty(precinctCode) ? "All precincts" : precinctCode) +
                "<br/><br/>No. of users attempted: " + results.TotalRequest +
                "<br/><br/>No. successfully processed users: " + results.ResponseList.Count(u => !u.Error) +
                "<br/><br/>No. of users where map responded with IsPossible = TRUE: " + results.ResponseList.Count(u => u.IsPossible) +
                "<br/><br/>No. of users where map responded with IsPossible = FALSE: " + results.ResponseList.Count(u => !u.IsPossible) +
                "<br/><br/>Users with Errors: " + usersWithErrorsString
            };

            EmailSender.SendEmail(email);
        }

        public static void ProcessUser(string userId, User profileToProcess = null)
        {
            var db = new ApplicationDbContext();
            var repo = new GriddingRepository();
            var user = db.Users.Include(u => u.Zone).First(u => u.Id == userId);

            if (profileToProcess != null)
            {
                user.Latitude = profileToProcess.Latitude;
                user.Longitude = profileToProcess.Longitude;
            }

            var zones = repo.GetZones().ToList();
            GetStatus(user, zones);
            db.SaveChanges();
        }

        public static bool CheckUserZoneForISPOrdering(User user)
        {
            var repo = new GriddingRepository();
            var zones = repo.GetZones().ToList();
            var response = false;

            UserSyncResponseDto result = GetStatus(user, zones);

            if (result.User != null && result.User.ZoneId != null)
            {
                var zone = zones.FirstOrDefault(z => z.ZoneId == result.User.ZoneId);
                //if the map result == true and Zone == true or Location == true
                if (result.IsPossible && zone != null && zone.AllowOrder)
                {
                    response = true;
                }
            }
            return response;
        }

        public static void CheckAndUpdateUserForZone(User modifiedUser, ApplicationDbContext db)
        {
            if (modifiedUser.Latitude != null && modifiedUser.Longitude != null)
            {
                ProcessUser(modifiedUser.Id, modifiedUser);
            }
        }
        #endregion

        #region private 
        private static DateTime Date { get; set; }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            date = date.AddSeconds(unixTimeStamp);
            return date;
        }

        private static List<User> GetUsersForZoneProcessing(string precinctCode, ApplicationDbContext db)
        {
            var manager = new UserManager<User>(new UserStore<User>(db));
            var users = db.Users
                .Include(u => u.Zone)
                .Include(u => u.Location)
                .Include(u => u.Estate)
                .Where(u => !u.IsDeleted);
            if (!string.IsNullOrEmpty(precinctCode))
            {
                users = users.Where(u => u.Location.PrecinctCode == precinctCode);
            }

            return users.ToList().Where(u => manager.IsInRole(u.Id, "Client")).ToList();
        }

        private static UserSyncResponseDto GetStatus(User user, List<Zone> zones)
        {
            string lat = user.Latitude.ToString().Replace(",", ".");
            string lng = user.Longitude.ToString().Replace(",", ".");
            const string url = "http://maps.frogfoot.net/ftth/check";

            var syncResponse = new UserSyncResponseDto
            {
                ZoneAssigned = false,
                IsPossible = false,
                User = user
            };

            if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lng))
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        string result = client.DownloadString(url + string.Format("?ll={0},{1}", lat, lng));

                        //if there is a zone that matches the zone code of the returned result the assign that zone to the user then save.
                        dynamic data = JObject.Parse(result);
                        string precinctCode = data["precinct-zone"];

                        if (data["possible"] == true && !string.IsNullOrEmpty(precinctCode))
                        {
                            syncResponse.IsPossible = true;
                            var zone = zones.FirstOrDefault(z => z.Code == precinctCode);
                            if (zone != null)
                            {
                                user.ZoneId = zone.ZoneId;
                                syncResponse.ZoneAssigned = true;
                                syncResponse.ErrorMessage = result + " USER-ZONE-ID: " + user.ZoneId;
                            }
                            else
                            {
                                user.ZoneId = null;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }

            }
            return syncResponse;
        }
        #endregion
    }

    public class ResponseModel
    {
        public ResponseModel()
        {
            ResponseList = new List<UserSyncResponseDto>();
        }

        public List<UserSyncResponseDto> ResponseList { get; set; }
        public int TotalRequest { get; set; }
    }
}
