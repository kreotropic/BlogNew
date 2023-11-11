namespace BlogNew.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPostModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Posts",
                c => new
                    {
                        PostId = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: true, maxLength: 128),
                        Title = c.String(),
                        Sinopse = c.String(),
                        Content = c.String(),
                        CreatedAt = c.DateTime(nullable: false, precision: 7),
                        UpdatedAt = c.DateTime(nullable: false, precision: 7),
                        IsPrivate = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.PostId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Posts", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.Posts", new[] { "UserId" });
            DropTable("dbo.Posts");
        }
    }
}
