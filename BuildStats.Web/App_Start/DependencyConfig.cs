using BuildStats.Core;
using BuildStats.Web.Config;
using Ninject;

namespace BuildStats.Web
{
    public sealed class DependencyConfig
    {
        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IBuildHistoryParser>().To<AppVeyorBuildHistoryParser>();
            kernel.Bind<IRestfulApiClient>().To<RestfulApiClient>();
            kernel.Bind<IBuildSystemFactory>().To<AppVeyorFactory>()
                .WithConstructorArgument("apiBuildHistoryUrlFormat", AppConfig.AppVeyor_API_BuildHistory_URL_Format);
            kernel.Bind<IBuildHistoryApiClient>().To<BuildHistoryApiClient>();
            kernel.Bind<IBuildStatistics>().To<BuildStatistics>();
            kernel.Bind<IChartConfig>().To<ChartConfig>();
        }
    }
}