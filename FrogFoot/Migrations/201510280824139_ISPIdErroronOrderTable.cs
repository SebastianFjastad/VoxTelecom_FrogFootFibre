namespace FrogFoot.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ISPIdErroronOrderTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Order", "ISP_ISPId1", "dbo.ISP");
            DropForeignKey("dbo.Order", "ISP_ISPId2", "dbo.ISP");
            DropForeignKey("dbo.Order", "ISP_ISPId", "dbo.ISP");
            DropIndex("dbo.Order", new[] { "ISP_ISPId" });
            DropIndex("dbo.Order", new[] { "ISP_ISPId1" });
            DropIndex("dbo.Order", new[] { "ISP_ISPId2" });
            DropColumn("dbo.Order", "ISPId");
            //DropColumn("dbo.Order", "ISPId");
            RenameColumn(table: "dbo.Order", name: "ISP_ISPId2", newName: "ISPId");
            //RenameColumn(table: "dbo.Order", name: "ISP_ISPId", newName: "ISPId");
            AlterColumn("dbo.Order", "ISPId", c => c.Int(nullable: true));
            //AlterColumn("dbo.Order", "ISPId", c => c.Int(nullable: false));
            CreateIndex("dbo.Order", "ISPId");
            AddForeignKey("dbo.Order", "ISPId", "dbo.ISP", "ISPId", cascadeDelete: false);
            DropColumn("dbo.Order", "ISP_ISPId1");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Order", "ISP_ISPId1", c => c.Int());
            DropForeignKey("dbo.Order", "ISPId", "dbo.ISP");
            DropIndex("dbo.Order", new[] { "ISPId" });
            AlterColumn("dbo.Order", "ISPId", c => c.Int());
            AlterColumn("dbo.Order", "ISPId", c => c.Int());
            RenameColumn(table: "dbo.Order", name: "ISPId", newName: "ISP_ISPId");
            RenameColumn(table: "dbo.Order", name: "ISPId", newName: "ISP_ISPId2");
            AddColumn("dbo.Order", "ISPId", c => c.Int(nullable: false));
            AddColumn("dbo.Order", "ISPId", c => c.Int(nullable: false));
            CreateIndex("dbo.Order", "ISP_ISPId2");
            CreateIndex("dbo.Order", "ISP_ISPId1");
            CreateIndex("dbo.Order", "ISP_ISPId");
            AddForeignKey("dbo.Order", "ISP_ISPId", "dbo.ISP", "ISPId");
            AddForeignKey("dbo.Order", "ISP_ISPId2", "dbo.ISP", "ISPId");
            AddForeignKey("dbo.Order", "ISP_ISPId1", "dbo.ISP", "ISPId");
        }
    }
}
