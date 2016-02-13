using System.Web.Mvc;

namespace BuildStats.Web.Config
{
    public static class MvcConfig
    {
        public static void Setup()
        {
            MvcHandler.DisableMvcResponseHeader = true;
        }
    }
}