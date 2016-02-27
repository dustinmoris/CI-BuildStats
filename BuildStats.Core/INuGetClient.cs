using System.Threading.Tasks;

namespace BuildStats.Core
{
    public interface INuGetClient
    {
        Task<NuGetPackageInfo> GetPackageInfo(string packageName);
    }
}