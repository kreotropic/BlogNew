namespace BlogNew.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserIsDisabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsDisabled", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IsDisabled");
        }
    }
}
