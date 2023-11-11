namespace BlogNew.Migrations
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BlogNew.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            ContextKey = "BlogNew.Models.ApplicationDbContext";
        }

        protected override void Seed(BlogNew.Models.ApplicationDbContext context)
        {
            context.Roles.AddOrUpdate(
                r => r.Name,
                new IdentityRole { Id = "1", Name = "User" },
                new IdentityRole { Id = "2", Name = "Admin" }
             );


            context.SaveChanges();
        }
    }
}
