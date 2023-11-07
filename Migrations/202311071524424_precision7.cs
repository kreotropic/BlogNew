namespace BlogNew.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class precision7 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Posts", "CreatedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.Posts", "UpdatedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Posts", "UpdatedAt", c => c.DateTime());
            AlterColumn("dbo.Posts", "CreatedAt", c => c.DateTime(nullable: false));
        }
    }
}
