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
                return await httpClient.GetStringAsync(url);
            }
        }
    }
}