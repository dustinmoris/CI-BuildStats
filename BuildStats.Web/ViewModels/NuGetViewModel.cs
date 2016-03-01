using BuildStats.Core;
using BuildStats.Web.Config;

namespace BuildStats.Web.ViewModels
{
    public sealed class NuGetViewModel
    {
        public NuGetViewModel(
            INuGetConfig config,
            NuGetPackageInfo packageInfo)
        {
            Config = config;
            PackageInfo = packageInfo;
        }

        public INuGetConfig Config { get; }
        public NuGetPackageInfo PackageInfo { get; }
    }
}