using BuildStats.Core;
using BuildStats.Core.BuildHistoryChart;
using BuildStats.Core.BuildHistoryChart.CircleCI;
using BuildStats.Web.Config;

namespace BuildStats.Web.Controllers
{
    public sealed class CircleCIController : BuildHistoryController
    {
        public CircleCIController(IBuildStatistics buildStatistics, IChartConfig chartConfig)
            : base(new CircleCIFactory().CreateBuildHistoryClient(), buildStatistics, chartConfig)
        {
        }
    }
}