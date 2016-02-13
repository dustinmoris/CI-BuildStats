using BuildStats.Core;
using BuildStats.Core.AppVeyor;
using BuildStats.Web.Config;

namespace BuildStats.Web.Controllers
{
    public sealed class AppVeyorController : BuildHistoryController
    {
        public AppVeyorController(IBuildStatistics buildStatistics, IChartConfig chartConfig) 
            : base(new AppVeyorFactory().CreateBuildHistoryClient(), buildStatistics, chartConfig)
        {
        }
    }
}