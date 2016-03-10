using System.Threading.Tasks;
using System.Web.Mvc;
using BuildStats.Core.PackageBadge.MyGet;
using BuildStats.Web.Config;
using BuildStats.Web.ViewModels;

namespace BuildStats.Web.Controllers
{
    public class MyGetController : Controller
    {
        private readonly IMyGetClient _mygetClient;
        private readonly IPackageBadgeConfig _badgeConfig;

        public MyGetController(IMyGetClient mygetClient, IPackageBadgeConfig badgeConfig)
        {
            _mygetClient = mygetClient;
            _badgeConfig = badgeConfig;
        }

        public async Task<ActionResult> Badge(string feedName, string packageName)
        {
            var packageInfo = await _mygetClient.GetPackageInfo(feedName, packageName);
            var viewModel = new PackageBadgeViewModel("myget", _badgeConfig, packageInfo);

            Response.ContentType = "image/svg+xml";
            return View(viewModel);
        }
    }
}