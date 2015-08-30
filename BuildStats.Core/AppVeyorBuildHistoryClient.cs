using System.Collections.Generic;
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

        public async Task<IList<Build>> GetBuilds(string account, string project, int buildCount)
        {
            var url = string.Format(_urlFormat, account, project, buildCount);
            var result = await _restfulApiClient.Get(url);
            return _parser.Parse(result);
        }
    }
}