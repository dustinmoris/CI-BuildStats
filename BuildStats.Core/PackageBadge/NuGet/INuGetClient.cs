using System.Threading.Tasks;

namespace BuildStats.Core.PackageBadge.NuGet
{
    public interface INuGetClient
    {
        Task<PackageInfo> GetPackageInfo(string packageName);
    }
}