namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EstateFlagsRemoved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Location", "AllowOrder", c => c.Boolean(nullable: false, defaultValue:true));
            DropColumn("dbo.Estate", "IsActive");
            DropColumn("dbo.Estate", "IsStaging");
            DropColumn("dbo.Location", "IsStaging");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Location", "IsStaging", c => c.Boolean(nullable: false));
            AddColumn("dbo.Estate", "IsStaging", c => c.Boolean(nullable: false));
            AddColumn("dbo.Estate", "IsActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.Location", "AllowOrder");
        }
    }
}
