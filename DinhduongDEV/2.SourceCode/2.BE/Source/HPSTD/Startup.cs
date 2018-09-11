using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HPSTD.Startup))]
namespace HPSTD
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
