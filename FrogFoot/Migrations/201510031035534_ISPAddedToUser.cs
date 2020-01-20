namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ISPAddedToUser : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.AspNetUsers", "ISPId");
            AddForeignKey("dbo.AspNetUsers", "ISPId", "dbo.ISP", "ISPId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUsers", "ISPId", "dbo.ISP");
            DropIndex("dbo.AspNetUsers", new[] { "ISPId" });
        }
    }
}
