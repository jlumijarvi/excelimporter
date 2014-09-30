namespace ExcelImporter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            //CreateTable(
            //    "dbo.Devices",
            //    c => new
            //        {
            //            Imei = c.String(nullable: false, maxLength: 128),
            //            Brand = c.String(nullable: false),
            //            Type = c.String(),
            //            Model = c.String(nullable: false),
            //            Owner_ID = c.String(maxLength: 128),
            //        })
            //    .PrimaryKey(t => t.Imei)
            //    .ForeignKey("dbo.People", t => t.Owner_ID)
            //    .Index(t => t.Owner_ID);
            
            //CreateTable(
            //    "dbo.People",
            //    c => new
            //        {
            //            ID = c.String(nullable: false, maxLength: 128),
            //            Name = c.String(nullable: false),
            //            Email = c.String(),
            //            BirthDate = c.DateTime(),
            //            Comments = c.String(),
            //        })
            //    .PrimaryKey(t => t.ID);
            
            //CreateTable(
            //    "dbo.ImportedFiles",
            //    c => new
            //        {
            //            Id = c.String(nullable: false, maxLength: 128),
            //            User = c.String(),
            //            Path = c.String(nullable: false),
            //            OriginalFileName = c.String(nullable: false),
            //            Created = c.DateTime(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Id);
            
            //CreateTable(
            //    "dbo.ImportedTables",
            //    c => new
            //        {
            //            Name = c.String(nullable: false, maxLength: 128),
            //            Type = c.String(nullable: false),
            //        })
            //    .PrimaryKey(t => t.Name);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Devices", "Owner_ID", "dbo.People");
            DropIndex("dbo.Devices", new[] { "Owner_ID" });
            DropTable("dbo.ImportedTables");
            DropTable("dbo.ImportedFiles");
            DropTable("dbo.People");
            DropTable("dbo.Devices");
        }
    }
}
