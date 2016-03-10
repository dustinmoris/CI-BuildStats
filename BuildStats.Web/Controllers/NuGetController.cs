using System.Threading.Tasks;
using System.Web.Mvc;
using BuildStats.Core;
using BuildStats.Core.PackageBadge.NuGet;
using BuildStats.Web.Config;
using BuildStats.Web.ViewModels;

namespace BuildStats.Web.Controllers
{
    public class NuGetController : Controller
    {
        private readonly INuGetClient _nugetClient;
        private readonly INuGetConfig _nugetConfig;

        public NuGetController(INuGetClient nugetClient, INuGetConfig nugetConfig)
        {
            _nugetClient = nugetClient;
            _nugetConfig = nugetConfig;
        }

        public async Task<ActionResult> Info(string packageName)
        {
            var packageInfo = await _nugetClient.GetPackageInfo(packageName);
            var viewModel = new NuGetViewModel(_nugetConfig, packageInfo);

            Response.ContentType = "image/svg+xml";
            return View(viewModel);
        }
    }
}