using System.Threading.Tasks;

namespace BuildStats.Core.Common
{
    public interface IRestfulApiClient
    {
        Task<string> Get(string url);
    }
}