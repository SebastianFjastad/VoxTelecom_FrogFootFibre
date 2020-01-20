namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PortalRemoved : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Order", "Portal_Portalld", "dbo.Portal");
            DropIndex("dbo.Order", new[] { "Portal_Portalld" });
            AlterColumn("dbo.ISP", "CellNo", c => c.String(maxLength: 20));
            DropColumn("dbo.Order", "Portal_Portalld");
            DropTable("dbo.Portal");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Portal",
                c => new
                    {
                        Portalld = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Province = c.String(maxLength: 100),
                        Suburb = c.String(maxLength: 100),
                        City = c.String(maxLength: 100),
                        PostalCode = c.String(maxLength: 10),
                    })
                .PrimaryKey(t => t.Portalld);
            
            AddColumn("dbo.Order", "Portal_Portalld", c => c.Int());
            AlterColumn("dbo.ISP", "CellNo", c => c.String(nullable: false, maxLength: 20));
            CreateIndex("dbo.Order", "Portal_Portalld");
            AddForeignKey("dbo.Order", "Portal_Portalld", "dbo.Portal", "Portalld");
        }
    }
}
