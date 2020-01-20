namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StagingFlagOnLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Estate", "IsActive", c => c.Boolean(nullable: true));
            AddColumn("dbo.Estate", "IsStaging", c => c.Boolean(nullable: false));
            AddColumn("dbo.Location", "IsStaging", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Location", "IsStaging");
            DropColumn("dbo.Estate", "IsStaging");
            DropColumn("dbo.Estate", "IsActive");
        }
    }
}
