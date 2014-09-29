using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExcelImporter.Startup))]
namespace ExcelImporter
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
