using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class NuGetClient : INuGetClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly ISerializer _serializer;

        public NuGetClient(IRestfulApiClient restfulApiClient, ISerializer serializer)
        {
            _restfulApiClient = restfulApiClient;
            _serializer = serializer;
        }

        public async Task<NuGetPackageInfo> GetPackageInfo(string packageName)
        {
            var url = $"https://api-v3search-0.nuget.org/query?q={packageName}&skip=0&take=1&prerelease=false";
            var content = await _restfulApiClient.Get(url);
            var searchResult = _serializer.Deserialize(content);

            var packageInfo = searchResult.data[0];

            var id = packageInfo.id;

            return new NuGetPackageInfo(
                packageInfo.id.Value.ToString(),
                packageInfo.version.Value.ToString(),
                (int)packageInfo.totalDownloads.Value);

        }
    }
}