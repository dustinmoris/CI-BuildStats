using System.Web.Mvc;

namespace BuildStats.Web
{
    public static class MvcConfig
    {
        public static void ApplyChanges()
        {
            MvcHandler.DisableMvcResponseHeader = true;
        }
    }
}