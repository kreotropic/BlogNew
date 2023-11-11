namespace BlogNew.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddThumbModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Thumbs",
                c => new
                    {
                        ThumbId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable:true, maxLength: 128),
                        PostId = c.Int(nullable: true),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ThumbId)
                .ForeignKey("dbo.Posts", t => t.PostId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId)
                .Index(t => t.PostId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Thumbs", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Thumbs", "PostId", "dbo.Posts");
            DropIndex("dbo.Thumbs", new[] { "PostId" });
            DropIndex("dbo.Thumbs", new[] { "UserId" });
            DropTable("dbo.Thumbs");
        }
    }
}
