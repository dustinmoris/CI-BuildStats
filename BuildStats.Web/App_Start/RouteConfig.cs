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
                url: "AppVeyor/Chart/{account}/{project}/{buildCount}",
                defaults: new { controller = "AppVeyor", action = "Chart", buildCount = UrlParameter.Optional }
            );

            //routes.MapRoute(
            //    name: "Default",
            //    url: "{controller}/{action}/{id}",
            //    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            //);

            routes.LowercaseUrls = true;
        }
    }
}
