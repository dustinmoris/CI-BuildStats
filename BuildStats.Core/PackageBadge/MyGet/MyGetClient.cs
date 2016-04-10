using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BuildStats.Core.Common;

namespace BuildStats.Core.PackageBadge.MyGet
{
    public sealed class MyGetClient : IMyGetClient
    {
        private readonly IRestfulApiClient _restfulApiClient;
        private readonly ISerializer _serializer;

        public MyGetClient(IRestfulApiClient restfulApiClient, ISerializer serializer)
        {
            _restfulApiClient = restfulApiClient;
            _serializer = serializer;
        }

        public async Task<PackageInfo> GetPackageInfo(string feedName, string packageName)
        {
            var filter = WebUtility.UrlEncode($"Id eq '{packageName}'");
            var url = $"https://www.myget.org/F/{feedName}/api/v2/Packages()?$filter={filter}&$orderby=Published desc&$top=10";
            var content = await _restfulApiClient.Get(url);
            var searchResult = _serializer.Deserialize(content);

            var packageInfo = searchResult.d[0];

            return new PackageInfo(
                packageInfo.Id.Value.ToString(),
                packageInfo.Version.Value.ToString(),
                (int)packageInfo.DownloadCount.Value);
        }
    }
}