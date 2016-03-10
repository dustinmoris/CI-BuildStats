using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BuildStats.Core.Common
{
    public sealed class RestfulApiClient : IRestfulApiClient
    {
        public async Task<string> Get(string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await httpClient.GetAsync(url);

                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}