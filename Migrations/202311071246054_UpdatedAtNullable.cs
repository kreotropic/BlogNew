namespace BlogNew.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedAtNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Posts", "UpdatedAt", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Posts", "UpdatedAt", c => c.DateTime(nullable: false));
        }
    }
}
