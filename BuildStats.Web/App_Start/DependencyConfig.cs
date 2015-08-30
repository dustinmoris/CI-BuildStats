using BuildStats.Core;
using BuildStats.Web.Config;
using Ninject;

namespace BuildStats.Web
{
    public static class DependencyConfig
    {
        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IBuildStatistics>().To<BuildStatistics>();
            kernel.Bind<IChartConfig>().To<ChartConfig>();
        }
    }
}