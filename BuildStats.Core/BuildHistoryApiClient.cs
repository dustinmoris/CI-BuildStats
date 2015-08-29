using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class BuildHistoryApiClient : IBuildHistoryApiClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly IBuildSystemFactory _buildSystemFactory;

        public BuildHistoryApiClient(IRestfulApiClient restfulApiClient, IBuildSystemFactory buildSystemFactory)
        {
            _restfulApiClient = restfulApiClient;
            _buildSystemFactory = buildSystemFactory;
        }

        public async Task<IList<Build>> GetBuilds(string account, string project, int buildCount)
        {
            var urlFormat = _buildSystemFactory.GetApiBuildHistoryUrlFormat();
            var url = string.Format(urlFormat, account, project, buildCount);
            var result = await _restfulApiClient.Get(url);
            var parser = _buildSystemFactory.CreateBuildHistoryParser();
            var builds = parser.ParseJson(result);
            return builds;
        }
    }
}