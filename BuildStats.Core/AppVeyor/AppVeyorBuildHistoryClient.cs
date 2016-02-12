using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildStats.Core.AppVeyor
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
            var url = $"https://ci.appveyor.com/api/projects/{account}/{project}/history?recordsNumber={buildCount}";

            if (!string.IsNullOrEmpty(branch))
                url = $"{url}&branch={branch}";

            var builds = new List<Build>();
            var attempts = 5;

            do
            {
                var result = await _restfulApiClient.Get(url);

                if (result == null)
                    break;

                var batch = _parser.Parse(result);

                if (batch == null || batch.Count == 0)
                    break;

                builds.AddRange(batch.Where(build => includeBuildsFromPullRequest || !build.FromPullRequest));
                url = $"{url}&startBuildId={batch[batch.Count - 1].BuildId}";

            } while (
                builds.Count < buildCount
                && --attempts > 0);

            return builds.Count > buildCount ? builds.Take(buildCount).ToList() : builds;
        }
    }
}