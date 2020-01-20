namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsCapped_Added : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ISPProduct", "IsCapped", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ISPProduct", "IsCapped");
        }
    }
}
