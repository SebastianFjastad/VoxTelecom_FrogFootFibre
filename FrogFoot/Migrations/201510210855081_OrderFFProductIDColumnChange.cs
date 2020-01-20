namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrderFFProductIDColumnChange : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.OrderFFProduct");
            DropColumn("dbo.OrderFFProduct", "FFOrderFFProductId");
            AddColumn("dbo.OrderFFProduct", "OrderFFProductId", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.OrderFFProduct", "OrderFFProductId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.OrderFFProduct", "FFOrderFFProductId", c => c.Int(nullable: false, identity: true));
            DropPrimaryKey("dbo.OrderFFProduct");
            DropColumn("dbo.OrderFFProduct", "OrderFFProductId");
            AddPrimaryKey("dbo.OrderFFProduct", "FFOrderFFProductId");
        }
    }
}
