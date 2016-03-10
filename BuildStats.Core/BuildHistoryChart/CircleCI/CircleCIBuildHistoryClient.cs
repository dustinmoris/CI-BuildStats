using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BuildStats.Core.Common;

namespace BuildStats.Core.BuildHistoryChart.CircleCI
{
    public sealed class CircleCIBuildHistoryClient : IBuildHistoryClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly IBuildHistoryParser _parser;

        public CircleCIBuildHistoryClient(IRestfulApiClient restfulApiClient, IBuildHistoryParser parser)
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
            var url = $"https://circleci.com/api/v1/project/{account}/{project}";

            if (!string.IsNullOrEmpty(branch))
                url = $"{url}/tree/{WebUtility.UrlEncode(branch)}";

            url = $"{url}?limit={buildCount}";

            var builds = new List<Build>();
            var attempt = 0;
            const int maxAttempts = 5;

            do
            {
                var result = await _restfulApiClient.Get(url);

                if (result == null)
                    break;

                var batch = _parser.Parse(result);

                if (batch == null || batch.Count == 0)
                    break;

                builds.AddRange(batch.Where(build => includeBuildsFromPullRequest || !build.FromPullRequest));
                url = $"{url}&offset={attempt}";

            } while (
                builds.Count < buildCount
                && ++attempt < maxAttempts);

            return builds.Count > buildCount ? builds.Take(buildCount).ToList() : builds;
        }
    }
}