using System.Web.Mvc;
using System.Web.Routing;
using BuildStats.Web.Config;

namespace BuildStats.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            MvcConfig.Setup();
            DependencyConfig.Setup();
        }
    }
}
