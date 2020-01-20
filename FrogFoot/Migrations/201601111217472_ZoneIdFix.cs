namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ZoneIdFix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Post", "ZoneId", c => c.Int());
            CreateIndex("dbo.Post", "ZoneId");
            AddForeignKey("dbo.Post", "ZoneId", "dbo.Zone", "ZoneId");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Post", "ZoneId", "dbo.Zone");
            DropIndex("dbo.Post", new[] { "ZoneId" });
            DropColumn("dbo.Post", "ZoneId");
        }
    }
}
