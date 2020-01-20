namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductCap : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ISPProduct", "Cap", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ISPProduct", "Cap");
        }
    }
}
