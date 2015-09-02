using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public sealed class RestfulApiClient : IRestfulApiClient
    {
        public async Task<string> Get(string url)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);

                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}