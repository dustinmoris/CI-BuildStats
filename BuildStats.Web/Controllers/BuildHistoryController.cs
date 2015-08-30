using System.Threading.Tasks;
using System.Web.Mvc;
using BuildStats.Core;
using BuildStats.Web.Config;
using BuildStats.Web.ViewModels;

namespace BuildStats.Web.Controllers
{
    public abstract class BuildHistoryController : Controller
    {
        private readonly IBuildHistoryClient _buildHistoryClient;
        private readonly IBuildStatistics _buildStatistics;
        private readonly IChartConfig _chartConfig;

        protected BuildHistoryController(IBuildHistoryClient buildHistoryClient, IBuildStatistics buildStatistics, IChartConfig chartConfig)
        {
            _buildHistoryClient = buildHistoryClient;
            _buildStatistics = buildStatistics;
            _chartConfig = chartConfig;
        }

        public async Task<ActionResult> Chart(string account, string project, int? buildCount = null, bool showStats = true)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(project))
                return new HttpNotFoundResult();

            var builds = await _buildHistoryClient.GetBuilds(account, project, buildCount ?? _chartConfig.DefaultBuildCount);
            var longestBuildTime = _buildStatistics.GetLongestBuildTime(builds);
            var shortestBuildTime = _buildStatistics.GetShortestBuildTime(builds);
            var averageBuildTime = _buildStatistics.GetAverageBuildTime(builds);
            var viewModel = new ChartViewModel(_chartConfig, builds, longestBuildTime, shortestBuildTime, averageBuildTime, showStats);

            Response.ContentType = "image/svg+xml";
            return View(viewModel);
        }
    }
}