using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class AppVeyorBuildHistoryClient : IBuildHistoryClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly IBuildHistoryParser _parser;
        private readonly string _urlFormat;

        public AppVeyorBuildHistoryClient(IRestfulApiClient restfulApiClient, IBuildHistoryParser parser, string urlFormat)
        {
            _restfulApiClient = restfulApiClient;
            _parser = parser;
            _urlFormat = urlFormat;
        }

        public async Task<IList<Build>> GetBuilds(string account, string project, string branch, int buildCount, bool includeBuildsFromPullRequest)
        {
            var url = string.Format(_urlFormat, account, project, branch, buildCount);
            var result = await _restfulApiClient.Get(url);

            if (result == null)
                return null;

            var builds = _parser.Parse(result);

            return !includeBuildsFromPullRequest ? builds.Where(b => !b.FromPullRequest).ToList() : builds;
        }
    }
}