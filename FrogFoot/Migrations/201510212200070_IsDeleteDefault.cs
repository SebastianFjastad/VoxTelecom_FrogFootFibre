namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsDeleteDefault : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Location", "IsDeleted", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Location", "IsDeleted");
        }
    }
}
