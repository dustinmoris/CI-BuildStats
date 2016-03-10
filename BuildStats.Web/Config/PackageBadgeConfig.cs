using System.Web.Configuration;

namespace BuildStats.Web.Config
{
    public sealed class PackageBadgeConfig : IPackageBadgeConfig
    {
        public int FontSize => int.Parse(WebConfigurationManager.AppSettings["NuGet_FontSize"]);
        public string FontFamily => WebConfigurationManager.AppSettings["NuGet_FontFamily"];
        public string FontColour => WebConfigurationManager.AppSettings["NuGet_FontColour"];
        public int BadgeHeight => int.Parse(WebConfigurationManager.AppSettings["NuGet_Badge_Height"]);
        public int BadgeCornerRadius => int.Parse(WebConfigurationManager.AppSettings["NuGet_Badge_CornerRadius"]);
        public string BackgroundColourTitle => WebConfigurationManager.AppSettings["NuGet_Badge_BackgroundColour_Title"];
        public string BackgroundColourVersion => WebConfigurationManager.AppSettings["NuGet_Badge_BackgroundColour_Version"];
        public string BackgroundColourDownloads => WebConfigurationManager.AppSettings["NuGet_Badge_BackgroundColour_Downloads"]; 
    }
}