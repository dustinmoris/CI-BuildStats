using System.Threading.Tasks;
using System.Web.Mvc;
using BuildStats.Core;
using BuildStats.Web.Config;
using BuildStats.Web.ViewModels;

namespace BuildStats.Web.Controllers
{
    public sealed class AppVeyorController : Controller
    {
        private readonly IBuildHistoryApiClient _buildHistoryApiClient;
        private readonly IBuildStatistics _buildStatistics;
        private readonly IChartConfig _chartConfig;

        public AppVeyorController(IBuildHistoryApiClient buildHistoryApiClient, IBuildStatistics buildStatistics, IChartConfig chartConfig)
        {
            _buildHistoryApiClient = buildHistoryApiClient;
            _buildStatistics = buildStatistics;
            _chartConfig = chartConfig;
        }

        public async Task<ActionResult> Chart(string account, string project, int? buildCount = null, bool showStats = true)
        {
            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(project))
                return new HttpNotFoundResult();

            var builds = await _buildHistoryApiClient.GetBuilds(account, project, buildCount ?? _chartConfig.DefaultBuildCount);
            var longestBuildTime = _buildStatistics.GetLongestBuildTime(builds);
            var shortestBuildTime = _buildStatistics.GetShortestBuildTime(builds);
            var averageBuildTime = _buildStatistics.GetAverageBuildTime(builds);
            var viewModel = new ChartViewModel(_chartConfig, builds, longestBuildTime, shortestBuildTime, averageBuildTime, showStats);

            Response.ContentType = "image/svg+xml";
            return View(viewModel);
        }
    }
}