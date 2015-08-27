using System.Web.Mvc;

namespace BuildStats.Web
{
    public sealed class MvcConfig
    {
        public static void ApplyChanges()
        {
            MvcHandler.DisableMvcResponseHeader = true;
        }
    }
}