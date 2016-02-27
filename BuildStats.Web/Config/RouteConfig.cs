using System.Web.Mvc;
using System.Web.Routing;

namespace BuildStats.Web.Config
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Chart",
                url: "{controller}/{action}/{account}/{project}",
                defaults: new { controller = "AppVeyor", action = "Chart" }
            );

            routes.MapRoute(
                name: "NuGet",
                url: "nuget/{packageName}",
                defaults: new { controller = "NuGet", action = "Info" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
            );

            routes.LowercaseUrls = true;
        }
    }
}
