namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ISPLogoIdColumnDrop : DbMigration
    {
        public override void Up()
        {
            //DropColumn("dbo.ISPProduct", "ISPLogoId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ISPProduct", "ISPLogoId", c => c.Int());
        }
    }
}
