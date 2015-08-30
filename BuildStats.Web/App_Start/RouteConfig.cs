using System.Web.Mvc;
using System.Web.Routing;

namespace BuildStats.Web
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{account}/{project}",
                defaults: new { controller = "AppVeyor", action = "Chart" }
            );

            routes.LowercaseUrls = true;
        }
    }
}
