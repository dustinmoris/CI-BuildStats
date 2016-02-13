using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using BuildStats.Core;

namespace BuildStats.Web.Config
{
    public static class DependencyConfig
    {
        public static void Setup()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<BuildStatistics>().As<IBuildStatistics>();
            builder.RegisterType<ChartConfig>().As<IChartConfig>();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            DependencyResolver.SetResolver(new AutofacDependencyResolver(builder.Build()));
        }
    }
}