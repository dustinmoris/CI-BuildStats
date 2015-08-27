using BuildStats.Core;
using BuildStats.Web.Config;
using Ninject;

namespace BuildStats.Web
{
    public sealed class DependencyConfig
    {
        public static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<IBuildHistoryParser>().To<BuildHistoryParser>();
            kernel.Bind<IRestfulApiClient>().To<RestfulApiClient>();
            kernel.Bind<IAppVeyorUrlHelper>().To<AppVeyorUrlHelper>()
                .WithConstructorArgument("appVeyorApiBuildHistoryUrlFormat", AppConfig.AppVeyor_API_BuildHistory_URL_Format);
            kernel.Bind<IAppVeyorApiClient>().To<AppVeyorApiClient>();
            kernel.Bind<IBuildStatistics>().To<BuildStatistics>();
            kernel.Bind<IChartConfig>().To<ChartConfig>();
        }
    }
}