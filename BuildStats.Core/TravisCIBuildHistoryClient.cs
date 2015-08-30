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

        public async Task<IList<Build>> GetBuilds(string account, string project, int buildCount)
        {
            var builds = new List<Build>();
            IList<Build> batch;

            do
            {
                var afterBuildNumber = builds.Count == 0 ? long.MaxValue : builds.Last().BuildNumber;
                var url = string.Format(_urlFormat, account, project, afterBuildNumber);
                var result = await _restfulApiClient.Get(url);
                batch = _parser.Parse(result);
                builds.AddRange(batch);
            } while (builds.Count < buildCount && batch.Count > 0);

            return builds.Count > buildCount ? builds.Take(buildCount).ToList() : builds;
        }
    }
}