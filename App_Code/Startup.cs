using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebProgram.Startup))]
namespace WebProgram
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
