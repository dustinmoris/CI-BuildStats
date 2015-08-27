using System.Threading.Tasks;

namespace BuildStats.Core
{
    public interface IRestfulApiClient
    {
        Task<string> Get(string url);
    }
}