using System.Web.Mvc;

namespace BuildStats.Web.Controllers
{
    public class TestsController : Controller
    {
        public ActionResult BuildHistoryChart()
        {
            return View();
        }

        public ActionResult NuGetBadge()
        {
            return View();
        }
    }
}