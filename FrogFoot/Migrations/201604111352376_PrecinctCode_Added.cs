namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PrecinctCode_Added : DbMigration
    {
        public override void Up()
        {
            RenameColumn("dbo.Location", "LocationCode", "PrecinctCode");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.Location", "PrecinctCode", "LocationCode");
        }
    }
}
