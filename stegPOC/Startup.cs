using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(stegPOC.Startup))]
namespace stegPOC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
