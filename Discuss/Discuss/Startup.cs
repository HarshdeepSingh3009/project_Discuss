using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Discuss.Startup))]
namespace Discuss
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
