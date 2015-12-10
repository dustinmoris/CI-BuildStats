using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class TravisCIBuildHistoryClient : IBuildHistoryClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly IBuildHistoryParser _parser;

        public TravisCIBuildHistoryClient(IRestfulApiClient restfulApiClient, IBuildHistoryParser parser)
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
            const double fixedAmountOfBuildsPerRequestForTravisCi = 25;
            const double factor = 5;
            var attempts = Math.Ceiling((buildCount / fixedAmountOfBuildsPerRequestForTravisCi) * factor);

            var url = $"https://api.travis-ci.org/repos/{account}/{project}/builds";

            var builds = new List<Build>();

            do
            {
                var result = await _restfulApiClient.Get(url);

                if (result == null)
                    break;

                var batch = _parser.Parse(result);

                if (batch == null || batch.Count == 0)
                    break;

                builds.AddRange(batch
                    .Where(build => string.IsNullOrEmpty(branch) || build.Branch == branch)
                    .Where(build => includeBuildsFromPullRequest || !build.FromPullRequest));
                url = $"{url}?after_number={batch[batch.Count -1].BuildNumber}";

            } while (
                builds.Count < buildCount 
                && --attempts > 0);

            return builds.Count > buildCount ? builds.Take(buildCount).ToList() : builds;
        }
    }
}