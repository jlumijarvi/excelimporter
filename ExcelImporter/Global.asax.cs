using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Owin;
using System.Threading;

namespace ExcelImporter
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (Context.User == null || !Context.User.Identity.IsAuthenticated)
                {
                    var signinManager = Context.GetOwinContext().GetUserManager<ApplicationSignInManager>();
                    var manager = Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
                    signinManager.SignIn(manager.FindByName("developer@foo.com"), true, true);
                }
            }
        }
    }
}
