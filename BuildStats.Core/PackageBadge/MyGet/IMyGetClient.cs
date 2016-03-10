using System.Threading.Tasks;

namespace BuildStats.Core.PackageBadge.MyGet
{
    public interface IMyGetClient
    {
        Task<PackageInfo> GetPackageInfo(string feedName, string packageName);
    }
}