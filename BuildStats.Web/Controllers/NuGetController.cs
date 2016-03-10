using System.Threading.Tasks;
using System.Web.Mvc;
using BuildStats.Core.PackageBadge.NuGet;
using BuildStats.Web.Config;
using BuildStats.Web.ViewModels;

namespace BuildStats.Web.Controllers
{
    public class NuGetController : Controller
    {
        private readonly INuGetClient _nugetClient;
        private readonly IPackageBadgeConfig _badgeConfig;

        public NuGetController(INuGetClient nugetClient, IPackageBadgeConfig badgeConfig)
        {
            _nugetClient = nugetClient;
            _badgeConfig = badgeConfig;
        }

        public async Task<ActionResult> Badge(string packageName, bool includePreReleases = false)
        {
            var packageInfo = await _nugetClient.GetPackageInfo(packageName, includePreReleases);
            var viewModel = new PackageBadgeViewModel("nuget", _badgeConfig, packageInfo);

            Response.ContentType = "image/svg+xml";
            return View(viewModel);
        }
    }
}