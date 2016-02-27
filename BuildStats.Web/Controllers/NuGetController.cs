using System.Threading.Tasks;
using System.Web.Mvc;
using BuildStats.Core;

namespace BuildStats.Web.Controllers
{
    public class NuGetController : Controller
    {
        private readonly INuGetClient _nugetClient;

        public NuGetController(INuGetClient nugetClient)
        {
            _nugetClient = nugetClient;
        }

        public async Task<ActionResult> Info(string packageName)
        {
            var packageInfo = await _nugetClient.GetPackageInfo(packageName);

            Response.ContentType = "image/svg+xml";
            return View(packageInfo);
        }
    }
}