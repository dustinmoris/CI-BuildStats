using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class AppVeyorApiClient : IAppVeyorApiClient
    {
        private readonly IAppVeyorUrlHelper _appVeyorUrlHelper;
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly IBuildHistoryParser _buildHistoryParser;

        public AppVeyorApiClient(IAppVeyorUrlHelper appVeyorUrlHelper, IRestfulApiClient restfulApiClient, IBuildHistoryParser buildHistoryParser)
        {
            _appVeyorUrlHelper = appVeyorUrlHelper;
            _restfulApiClient = restfulApiClient;
            _buildHistoryParser = buildHistoryParser;
        }

        public async Task<IList<Build>> GetBuilds(string account, string project, int buildCount)
        {
            var url = _appVeyorUrlHelper.CreateBuildHistoryUrl(account, project, buildCount);
            var result = await _restfulApiClient.Get(url);
            var builds = _buildHistoryParser.ParseJson(result);
            return builds;
        }
    }
}