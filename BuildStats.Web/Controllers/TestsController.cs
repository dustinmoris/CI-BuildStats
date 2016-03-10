using System.Web.Mvc;

namespace BuildStats.Web.Controllers
{
    public class TestsController : Controller
    {
        public ActionResult BuildHistoryChart()
        {
            return View();
        }

        public ActionResult NuGet()
        {
            return View();
        }

        public ActionResult MyGet()
        {
            return View();
        }
    }
}