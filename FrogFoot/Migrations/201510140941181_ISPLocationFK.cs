namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ISPLocationFK : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ISPLocation", "ISPId", "dbo.ISP");
            DropForeignKey("dbo.ISPLocation", "LocationId", "dbo.Location");
            DropForeignKey("dbo.ISPProduct", "ISPLocation_ISPLocationId", "dbo.ISPLocation");
            DropIndex("dbo.ISPProduct", new[] { "ISPLocation_ISPLocationId" });
            DropIndex("dbo.ISPLocation", new[] { "ISPId" });
            DropIndex("dbo.ISPLocation", new[] { "LocationId" });

            DropColumn("dbo.ISPProduct", "ISPLocation_ISPLocationId");

            CreateTable(
                "dbo.ISPEstateProduct",
                c => new
                    {
                        ISPEstateProductId = c.Int(nullable: false, identity: true),
                        ISPProductId = c.Int(nullable: false),
                        ISPId = c.Int(nullable: false),
                        EstateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ISPEstateProductId)
                .ForeignKey("dbo.Estate", t => t.EstateId, cascadeDelete: false)
                .ForeignKey("dbo.ISP", t => t.ISPId, cascadeDelete: false)
                .ForeignKey("dbo.ISPProduct", t => t.ISPProductId, cascadeDelete: false)
                .Index(t => t.ISPProductId)
                .Index(t => t.ISPId)
                .Index(t => t.EstateId);
            
            CreateTable(
                "dbo.ISPLocationProduct",
                c => new
                    {
                        ISPLocationProductId = c.Int(nullable: false, identity: true),
                        ISPProductId = c.Int(nullable: false),
                        ISPId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ISPLocationProductId)
                .ForeignKey("dbo.ISP", t => t.ISPId, cascadeDelete: false)
                .ForeignKey("dbo.ISPProduct", t => t.ISPProductId, cascadeDelete: false)
                .ForeignKey("dbo.Location", t => t.LocationId, cascadeDelete: false)
                .Index(t => t.ISPProductId)
                .Index(t => t.ISPId)
                .Index(t => t.LocationId);
            
            DropTable("dbo.ISPLocation");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ISPLocation",
                c => new
                    {
                        ISPLocationId = c.Int(nullable: false, identity: true),
                        ISPId = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ISPLocationId);
            
            AddColumn("dbo.ISPProduct", "ISPLocation_ISPLocationId", c => c.Int());
            DropForeignKey("dbo.ISPEstateProduct", "ISPProductId", "dbo.ISPProduct");
            DropForeignKey("dbo.ISPEstateProduct", "ISPId", "dbo.ISP");
            DropForeignKey("dbo.ISPEstateProduct", "EstateId", "dbo.Estate");
            DropForeignKey("dbo.ISPLocationProduct", "LocationId", "dbo.Location");
            DropForeignKey("dbo.ISPLocationProduct", "ISPProductId", "dbo.ISPProduct");
            DropForeignKey("dbo.ISPLocationProduct", "ISPId", "dbo.ISP");
            DropIndex("dbo.ISPLocationProduct", new[] { "LocationId" });
            DropIndex("dbo.ISPLocationProduct", new[] { "ISPId" });
            DropIndex("dbo.ISPLocationProduct", new[] { "ISPProductId" });
            DropIndex("dbo.ISPEstateProduct", new[] { "EstateId" });
            DropIndex("dbo.ISPEstateProduct", new[] { "ISPId" });
            DropIndex("dbo.ISPEstateProduct", new[] { "ISPProductId" });
            DropTable("dbo.ISPLocationProduct");
            DropTable("dbo.ISPEstateProduct");
            CreateIndex("dbo.ISPLocation", "LocationId");
            CreateIndex("dbo.ISPLocation", "ISPId");
            CreateIndex("dbo.ISPProduct", "ISPLocation_ISPLocationId");
            AddForeignKey("dbo.ISPProduct", "ISPLocation_ISPLocationId", "dbo.ISPLocation", "ISPLocationId");
            AddForeignKey("dbo.ISPLocation", "LocationId", "dbo.Location", "LocationId", cascadeDelete: true);
            AddForeignKey("dbo.ISPLocation", "ISPId", "dbo.ISP", "ISPId", cascadeDelete: true);
        }
    }
}
