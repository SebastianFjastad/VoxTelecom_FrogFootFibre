namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Coordinates : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "Latitude", c => c.String());
            AlterColumn("dbo.AspNetUsers", "Longitude", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "Longitude", c => c.Decimal(precision: 18, scale: 2));
            AlterColumn("dbo.AspNetUsers", "Latitude", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}
