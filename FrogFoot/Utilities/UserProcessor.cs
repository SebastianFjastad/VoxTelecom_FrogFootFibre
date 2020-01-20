using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using OfficeOpenXml;

namespace FrogFoot.Utilities
{
    public static class UserProcessor
    {
        private static ApplicationDbContext db = Db.GetInstance();

        public static void Process()
        {
            using (ExcelPackage package = new ExcelPackage(new FileInfo(@"C:\Users\basti_000\Desktop\Projects\FrogFoot\De Zalze\UsersToLoad-2016-7-21.xlsx")))
            {
                var manager = new UserManager<User>(new UserStore<User>(db));
                manager.UserValidator = new UserValidator<User>(manager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true,
                };

                var usersSheet = package.Workbook.Worksheets[1];
                var fileName = DateTime.Now.ToString("yyyy-MM-dd-hh-mm") + ".xlsx";
                var outputDir = HttpContext.Current.Server.MapPath("~/UsersFile/");
                var file = new FileInfo(outputDir + fileName);

                using (var newFile = new ExcelPackage(file))
                {
                    ExcelWorksheet ws = newFile.Workbook.Worksheets.Add("Users");

                    for (int i = 1; i <= usersSheet.Dimension.End.Row; i++)
                    {
                        if (i != 1)
                        {
                            var firstName = usersSheet.Cells[i, 1].Value;
                            var lastName = usersSheet.Cells[i, 2].Value;
                            var email = usersSheet.Cells[i, 3].Value;
                            var cellNo = usersSheet.Cells[i, 4].Value;
                            var landline = usersSheet.Cells[i, 5].Value;
                            var address = usersSheet.Cells[i, 6].Value;
                            var suburb = usersSheet.Cells[i, 7].Value;
                            var estate = usersSheet.Cells[i, 8].Value;
                            var zone = usersSheet.Cells[i, 9].Value;
                            var lat = usersSheet.Cells[i, 10].Value;
                            var lng = usersSheet.Cells[i, 11].Value;

                            var lat1 = Convert.ToDouble(lat);

                            var password = PasswordGenerator.Generate(6);

                            var user = new User
                            {
                                FirstName = firstName as string,
                                LastName = lastName as string,
                                Email = email as string,
                                UserName = email as string,
                                EmailConfirmed = true,
                                PhoneNumber = cellNo as string,
                                Landline = landline as string,
                                Address = address as string,
                                SuburbString = suburb as string,
                                EstateString = estate as string,
                                TempPassword = password as string,
                                Latitude = lat1,
                                Longitude = lng as double?,
                                Temp = true,
                            };

                            try
                            {
                                IdentityResult result = manager.Create(user, password);
                                if (result.Succeeded)
                                {
                                    ws.Cells[i, 1].Value = user.Id;
                                    ws.Cells[i, 2].Value = user.FirstName;
                                    ws.Cells[i, 3].Value = user.LastName;
                                    ws.Cells[i, 4].Value = user.Email;
                                    ws.Cells[i, 5].Value = user.PhoneNumber;
                                    ws.Cells[i, 6].Value = user.Landline;
                                    ws.Cells[i, 7].Value = user.Address;
                                    ws.Cells[i, 8].Value = user.SuburbString;
                                    ws.Cells[i, 9].Value = user.EstateString;
                                    ws.Cells[i, 10].Value = zone;
                                    ws.Cells[i, 11].Value = user.Latitude;
                                    ws.Cells[i, 12].Value = user.Longitude;
                                    ws.Cells[i, 13].Value = password;
                                    ws.Cells[i, 14].Value = user.PasswordHash;
                                    ws.Cells[i, 15].Value = user.SecurityStamp;
                                }
                                else
                                {
                                    Debug.WriteLine(
                                        $"Id:{user.Id}, Email: {user.Email}, FirstName: {user.FirstName}, LastName: {user.LastName}, Error: {result.Errors}");
                                }

                            }
                            catch (DbEntityValidationException dbEx)
                            {
                                foreach (var validationErrors in dbEx.EntityValidationErrors)
                                {
                                    foreach (var validationError in validationErrors.ValidationErrors)
                                    {
                                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                                    }
                                }
                            }
                        } 
                    }
                    newFile.Save();
                }
            }
        }
    }
}