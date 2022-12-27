using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NetStarter.Startup))]
namespace NetStarter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
