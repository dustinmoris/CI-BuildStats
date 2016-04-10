using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BuildStats.Core.Common;

namespace BuildStats.Core.PackageBadge.NuGet
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

        public async Task<PackageInfo> GetPackageInfo(string packageName, bool includePreReleases)
        {
            var url = $"https://api-v3search-0.nuget.org/query?q={packageName}&skip=0&take=10&prerelease={includePreReleases}";
            var content = await _restfulApiClient.Get(url);
            var searchResult = _serializer.Deserialize(content);

            var packageInfo = (searchResult.data as IEnumerable<dynamic>).First(
                x => x.id.Value.ToString()
                .Equals(packageName, StringComparison.InvariantCultureIgnoreCase));

            return new PackageInfo(
                packageInfo.id.Value.ToString(),
                packageInfo.version.Value.ToString(),
                (int)packageInfo.totalDownloads.Value);
        }
    }
}