using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlogNew.Startup))]
namespace BlogNew
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
