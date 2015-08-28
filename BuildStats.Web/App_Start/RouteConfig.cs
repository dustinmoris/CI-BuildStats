using System.Web.Mvc;
using System.Web.Routing;

namespace BuildStats.Web
{
    public sealed class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "AppVeyor_Chart",
                url: "AppVeyor/Chart/{account}/{project}",
                defaults: new { controller = "AppVeyor", action = "Chart" }
            );

            routes.LowercaseUrls = true;
        }
    }
}
