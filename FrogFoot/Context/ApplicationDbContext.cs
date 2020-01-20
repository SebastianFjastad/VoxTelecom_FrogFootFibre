using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using FrogFoot.Areas.Home.Models;
using FrogFoot.Entities;
using FrogFoot.Migrations;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FrogFoot.Context
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext()
            : base("name=FrogFootDb")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Configuration>("FrogFootDb"));
        }

        public virtual DbSet<FFProduct> FFProducts { get; set; }
        public virtual DbSet<ISP> ISPs { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderFFProduct> OrderFFProducts { get; set; }
        public virtual DbSet<ISPProduct> ISPProducts { get; set; }
        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<Estate> Estates { get; set; }
        public virtual DbSet<ISPEstateDiscount> ISPEstateDiscounts { get; set; }
        public virtual DbSet<ISPLocationProduct> ISPLocationProducts { get; set; }
        public virtual DbSet<ISPEstateProduct> ISPEstateProducts { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Zone> Zones { get; set; }
        public virtual DbSet<Portal> Portals { get; set; }
        public virtual DbSet<Url> Urls { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<Special> Specials { get; set; }
        public virtual DbSet<ContactMethod> ContactMethods { get; set; }
        public virtual DbSet<ClientContactMethod> ClientContactMethods { get; set; }
        public virtual DbSet<ClientISPContact> ClientISPContacts { get; set; }
        public virtual DbSet<ISPClientContact> ISPClientContacts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
