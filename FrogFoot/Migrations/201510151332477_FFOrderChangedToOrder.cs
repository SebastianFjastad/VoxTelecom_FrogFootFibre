namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FFOrderChangedToOrder : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.FFOrderFFProduct", "FFOrderId", "dbo.FFOrder");
            DropForeignKey("dbo.OrderAction", "FFOrderId", "dbo.FFOrder");
            DropForeignKey("dbo.FFOrder", "ISPId", "dbo.ISP");
            DropForeignKey("dbo.Order", "ISPId", "dbo.ISP");
            RenameTable(name: "dbo.FFOrderFFProduct", newName: "OrderFFProduct");
            DropIndex("dbo.FFOrder", new[] { "ISPId" });
            DropIndex("dbo.FFOrder", new[] { "LocationId" });
            DropIndex("dbo.FFOrder", new[] { "Portal_Portalld" });
            DropIndex("dbo.OrderFFProduct", new[] { "FFOrderId" });
            DropIndex("dbo.OrderAction", new[] { "FFOrderId" });
            DropIndex("dbo.Order", new[] { "ISPId" });
            DropTable("dbo.FFOrder");
            AddColumn("dbo.OrderAction", "OrderId", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "PortalId", c => c.Int());
            AddColumn("dbo.Order", "FFProductId", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "LocationId", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "SubscriberName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Order", "Address", c => c.String(nullable: false));
            AddColumn("dbo.Order", "ClientCode", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.Order", "CreatedDate", c => c.DateTime(nullable: false, storeType: "date"));
            AddColumn("dbo.Order", "ContractTerm", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "Latitude", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Order", "Longitude", c => c.Decimal(precision: 18, scale: 2));
            AddColumn("dbo.Order", "ISP_ISPId", c => c.Int());
            AddColumn("dbo.Order", "Portal_Portalld", c => c.Int());
            AddColumn("dbo.Order", "ISP_ISPId1", c => c.Int());
            AddColumn("dbo.Order", "ISP_ISPId2", c => c.Int());
            AddColumn("dbo.OrderFFProduct", "OrderId", c => c.Int(nullable: false));
            CreateIndex("dbo.Order", "LocationId");
            CreateIndex("dbo.Order", "ISP_ISPId");
            CreateIndex("dbo.Order", "Portal_Portalld");
            CreateIndex("dbo.Order", "ISP_ISPId1");
            CreateIndex("dbo.Order", "ISP_ISPId2");
            CreateIndex("dbo.OrderAction", "OrderId");
            CreateIndex("dbo.OrderFFProduct", "OrderId");
            AddForeignKey("dbo.OrderAction", "OrderId", "dbo.Order", "OrderId", cascadeDelete: true);
            AddForeignKey("dbo.OrderFFProduct", "OrderId", "dbo.Order", "OrderId", cascadeDelete: true);
            AddForeignKey("dbo.Order", "ISP_ISPId1", "dbo.ISP", "ISPId");
            AddForeignKey("dbo.Order", "ISP_ISPId2", "dbo.ISP", "ISPId");
            AddForeignKey("dbo.Order", "ISP_ISPId", "dbo.ISP", "ISPId");
            DropColumn("dbo.OrderAction", "FFOrderId");
            DropColumn("dbo.Order", "OrderCode");
            DropColumn("dbo.OrderFFProduct", "FFOrderId");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.FFOrder",
                c => new
                    {
                        FFOrderId = c.Int(nullable: false, identity: true),
                        PortalId = c.Int(),
                        FFProductId = c.Int(nullable: false),
                        ISPId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        LocationId = c.Int(nullable: false),
                        SubscriberName = c.String(nullable: false, maxLength: 100),
                        Address = c.String(nullable: false),
                        ClientCode = c.String(nullable: false, maxLength: 50),
                        CreatedDate = c.DateTime(nullable: false, storeType: "date"),
                        ContractTerm = c.Int(nullable: false),
                        Latitude = c.Decimal(precision: 18, scale: 2),
                        Longitude = c.Decimal(precision: 18, scale: 2),
                        Portal_Portalld = c.Int(),
                    })
                .PrimaryKey(t => t.FFOrderId);
            
            AddColumn("dbo.OrderFFProduct", "FFOrderId", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "OrderCode", c => c.String(maxLength: 100));
            AddColumn("dbo.OrderAction", "FFOrderId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Order", "ISP_ISPId", "dbo.ISP");
            DropForeignKey("dbo.Order", "ISP_ISPId2", "dbo.ISP");
            DropForeignKey("dbo.Order", "ISP_ISPId1", "dbo.ISP");
            DropForeignKey("dbo.OrderFFProduct", "OrderId", "dbo.Order");
            DropForeignKey("dbo.OrderAction", "OrderId", "dbo.Order");
            DropIndex("dbo.OrderFFProduct", new[] { "OrderId" });
            DropIndex("dbo.OrderAction", new[] { "OrderId" });
            DropIndex("dbo.Order", new[] { "ISP_ISPId2" });
            DropIndex("dbo.Order", new[] { "ISP_ISPId1" });
            DropIndex("dbo.Order", new[] { "Portal_Portalld" });
            DropIndex("dbo.Order", new[] { "ISP_ISPId" });
            DropIndex("dbo.Order", new[] { "LocationId" });
            DropColumn("dbo.OrderFFProduct", "OrderId");
            DropColumn("dbo.Order", "ISP_ISPId2");
            DropColumn("dbo.Order", "ISP_ISPId1");
            DropColumn("dbo.Order", "Portal_Portalld");
            DropColumn("dbo.Order", "ISP_ISPId");
            DropColumn("dbo.Order", "Longitude");
            DropColumn("dbo.Order", "Latitude");
            DropColumn("dbo.Order", "ContractTerm");
            DropColumn("dbo.Order", "CreatedDate");
            DropColumn("dbo.Order", "ClientCode");
            DropColumn("dbo.Order", "Address");
            DropColumn("dbo.Order", "SubscriberName");
            DropColumn("dbo.Order", "LocationId");
            DropColumn("dbo.Order", "Status");
            DropColumn("dbo.Order", "FFProductId");
            DropColumn("dbo.Order", "PortalId");
            DropColumn("dbo.OrderAction", "OrderId");
            CreateIndex("dbo.Order", "ISPId");
            CreateIndex("dbo.OrderAction", "FFOrderId");
            CreateIndex("dbo.OrderFFProduct", "FFOrderId");
            CreateIndex("dbo.FFOrder", "Portal_Portalld");
            CreateIndex("dbo.FFOrder", "LocationId");
            CreateIndex("dbo.FFOrder", "ISPId");
            AddForeignKey("dbo.Order", "ISPId", "dbo.ISP", "ISPId", cascadeDelete: true);
            AddForeignKey("dbo.FFOrder", "ISPId", "dbo.ISP", "ISPId", cascadeDelete: true);
            AddForeignKey("dbo.OrderAction", "FFOrderId", "dbo.FFOrder", "FFOrderId", cascadeDelete: true);
            AddForeignKey("dbo.FFOrderFFProduct", "FFOrderId", "dbo.FFOrder", "FFOrderId", cascadeDelete: true);
            RenameTable(name: "dbo.OrderFFProduct", newName: "FFOrderFFProduct");
        }
    }
}
