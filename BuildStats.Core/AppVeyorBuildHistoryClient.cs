using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class AppVeyorBuildHistoryClient : IBuildHistoryClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly IBuildHistoryParser _parser;

        public AppVeyorBuildHistoryClient(IRestfulApiClient restfulApiClient, IBuildHistoryParser parser)
        {
            _restfulApiClient = restfulApiClient;
            _parser = parser;
        }

        public async Task<IList<Build>> GetBuilds(
            string account,
            string project,
            int buildCount,
            string branch = null,
            bool includeBuildsFromPullRequest = true)
        {
            var urlFormat = "https://ci.appveyor.com/api/projects/{0}/{1}/history?recordsNumber={2}";

            if (!string.IsNullOrEmpty(branch))
                urlFormat = $"{urlFormat}&branch={branch}";

            var url = string.Format(urlFormat, account, project, buildCount);
            var result = await _restfulApiClient.Get(url);

            if (result == null)
                return null;

            var builds = _parser.Parse(result);

            return !includeBuildsFromPullRequest ? builds.Where(b => !b.FromPullRequest).ToList() : builds;
        }
    }
}