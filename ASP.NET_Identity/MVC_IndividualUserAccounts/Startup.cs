using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MVC_IndividualUserAccounts.Startup))]
namespace MVC_IndividualUserAccounts
{
    public partial class Startup 
    {
        public void Configuration(IAppBuilder app) 
        {
            ConfigureAuth(app);
        }
    }
}
