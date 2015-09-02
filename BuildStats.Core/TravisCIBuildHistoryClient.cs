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
        private readonly string _urlFormat;

        public TravisCIBuildHistoryClient(IRestfulApiClient restfulApiClient, IBuildHistoryParser parser, string urlFormat)
        {
            _restfulApiClient = restfulApiClient;
            _parser = parser;
            _urlFormat = urlFormat;
        }

        public async Task<IList<Build>> GetBuilds(string account, string project, string branch, int buildCount)
        {
            // The TravisCI Rest API does not offer a parameter to filter builds per branch
            // or to retrieve a certain amount of builds. Therefore it needs to be consumed 
            // iteratively until the desired amount of builds has been retrieved.

            var builds = new List<Build>();
            IList<Build> batch;

            // Scenario:
            // A project has a large amount of builds across all branches (1000+).
            // Then the user specifies buildCount to be 25 and a branch which has less
            // than 25 builds (e.g. new branch).
            // Now the below logic will loop through all builds until it realises that
            // it cannot fill up the builds list with 25 builds of the specified branch.
            // maxAttempts is a hard limit on web requests to try and fill up the list.
            const double buildsPerRequest = 25;
            const double factor = 5;
            var maxAttempts = Math.Ceiling((buildCount / buildsPerRequest) * factor);

            do
            {
                var afterBuildNumber = builds.Count == 0 ? long.MaxValue : builds.Last().BuildNumber;
                var url = string.Format(_urlFormat, account, project, afterBuildNumber);
                var result = await _restfulApiClient.Get(url);

                if (result == null)
                    return null;

                batch = _parser.Parse(result);
                builds.AddRange(batch.Where(b => b.Branch == branch));
            } while (builds.Count < buildCount && batch.Count > 0 && --maxAttempts > 0);

            return builds.Count > buildCount ? builds.Take(buildCount).ToList() : builds;
        }
    }
}