using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Context;
using FrogFoot.Entities;
using FrogFoot.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FrogFoot.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(ApplicationDbContext context)
        {
            var store = new RoleStore<IdentityRole>(context);
            var roleManager = new RoleManager<IdentityRole>(store);

            #region Roles and Admin
            //if (!context.Roles.Any(r => r.Name == "Admin"))
            //{
            //    var role = new IdentityRole { Name = "Admin" };
            //    roleManager.Create(role);
            //}

            //if (!context.Roles.Any(r => r.Name == "ISPUser"))
            //{
            //    var role = new IdentityRole { Name = "ISPUser" };
            //    roleManager.Create(role);
            //}
            //if (!context.Roles.Any(r => r.Name == "FFUser"))
            //{
            //    var role = new IdentityRole { Name = "FFUser" };
            //    roleManager.Create(role);
            //}
            //if (!context.Roles.Any(r => r.Name == "Client"))
            //{
            //    var role = new IdentityRole { Name = "Client" };
            //    roleManager.Create(role);
            //}
            //if (!context.Roles.Any(r => r.Name == "Champ"))
            //{
            //    var role = new IdentityRole { Name = "Champ" };
            //    roleManager.Create(role);
            //}
            //if (!context.Roles.Any(r => r.Name == "FFManager"))
            //{
            //    var role = new IdentityRole { Name = "FFManager" };
            //    roleManager.Create(role);
            //}
            //if (!context.Roles.Any(r => r.Name == "Comms"))
            //{
            //    var role = new IdentityRole { Name = "Comms" };
            //    roleManager.Create(role);
            //}

            //var manager = new UserManager<User>(
            //    new UserStore<User>(
            //        context));

            //var adminExists = manager.FindByEmail("admin@admin.com");
            //if (adminExists == null)
            //{
            //    var user = new User
            //    {
            //        FirstName = "Admin",
            //        LastName = "Admin",
            //        Email = "admin@admin.com",
            //        UserName = "admin@admin.com",
            //        PhoneNumber = "11223344",
            //        EmailConfirmed = true
            //    };
            //    manager.Create(user, "admin123");
            //    manager.AddToRole(user.Id, "Admin");
            //}

            #endregion

            #region Products

            //context.FFProducts.AddOrUpdate(p => p.Name,
            //    new FFProduct
            //    {
            //        Name = "Access link (10 Mbps)",
            //        UnitPriceOnceOff = 1500,
            //        UnitPriceMonthly = 350,
            //        M2MUnitPriceOnceOff = 1000,
            //        M2MUnitPriceMonthly = 400,
            //        Type = ProductType.LineSpeed,
            //        LineSpeed = LineSpeed.TenMbps
            //    },
            //    new FFProduct
            //    {
            //        Name = "Access link (20 Mbps)",
            //        UnitPriceOnceOff = 1500,
            //        UnitPriceMonthly = 400,
            //        M2MUnitPriceOnceOff = 1000,
            //        M2MUnitPriceMonthly = 450,
            //        Type = ProductType.LineSpeed,
            //        LineSpeed = LineSpeed.TwentyMbps
            //    },
            //    new FFProduct
            //    {
            //        Name = "Access link (50 Mbps)",
            //        UnitPriceOnceOff = 1500,
            //        UnitPriceMonthly = 450,
            //        M2MUnitPriceOnceOff = 1000,
            //        M2MUnitPriceMonthly = 500,
            //        Type = ProductType.LineSpeed,
            //        LineSpeed = LineSpeed.FiftyMbps
            //    },
            //    new FFProduct
            //    {
            //        Name = "Access link (100 Mbps)",
            //        UnitPriceOnceOff = 1500,
            //        UnitPriceMonthly = 500,
            //        M2MUnitPriceOnceOff = 1000,
            //        M2MUnitPriceMonthly = 550,
            //        Type = ProductType.LineSpeed,
            //        LineSpeed = LineSpeed.HundredMbps
            //    },
            //    new FFProduct
            //    {
            //        Name = "Access link (1 Gbps)",
            //        UnitPriceOnceOff = 1500,
            //        UnitPriceMonthly = 1000,
            //        M2MUnitPriceOnceOff = 1000,
            //        M2MUnitPriceMonthly = 1050,
            //        Type = ProductType.LineSpeed,
            //        LineSpeed = LineSpeed.OneGps
            //    });

            //new products to add again 
            //new FFProduct
            //{
            //    Name = "Battery Backup and Network Alerts",
            //    UnitPriceMonthly = 75,
            //    Type = ProductType.Option
            //},

            //new FFProduct
            //{
            //    Name = "32 Kbps Voice Channel",
            //    UnitPriceMonthly = 50,
            //    Type = ProductType.Quantity
            //}


            //new FFProduct
            //     {
            //         Name = "CCTV and RF Overlay",
            //         UnitPriceOnceOff = 1500,
            //         UnitPriceMonthly = 100,
            //         Type = ProductType.Quantity
            //     },

            // new FFProduct
            // {
            //     Name = "Relocation Fee",
            //     UnitPriceOnceOff = 1500,
            //     Type = ProductType.Option
            // },
            // new FFProduct
            // {
            //     Name = "Call Out Fee",
            //     UnitPriceOnceOff = 750,
            //     Type = ProductType.Option
            // }

            #endregion

            #region Specials
            //context.Specials.AddOrUpdate(s => s.TimePeriodName, 
            //    new Special
            //    {
            //        Discount = Discount.Hundred,
            //        TimePeriodName = "3 Months",
            //        TimePeriodMonths = 3,
            //        SpecialLineSpeed = LineSpeed.OneGps
            //    },
            //    new Special
            //    {
            //        Discount = Discount.Fifty,
            //        TimePeriodName = "3 Months",
            //        TimePeriodMonths = 3,
            //        SpecialLineSpeed = LineSpeed.HundredMbps
            //    });
            #endregion

            #region Contact Types
            //context.ContactMethods.AddOrUpdate(x => x.Name,
            //    new ContactMethod{ Name = "Cell" },
            //    new ContactMethod{ Name = "Email" },
            //    new ContactMethod{ Name = "Landline" },
            //    new ContactMethod{ Name = "SMS" }
            //    );

            #endregion

            SaveChanges(context);
        }

        public void SaveChanges(ApplicationDbContext context)
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
