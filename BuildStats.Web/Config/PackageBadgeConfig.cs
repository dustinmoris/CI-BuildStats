using System.Web.Configuration;

namespace BuildStats.Web.Config
{
    public sealed class PackageBadgeConfig : IPackageBadgeConfig
    {
        public int FontSize => int.Parse(WebConfigurationManager.AppSettings["Package_FontSize"]);
        public string FontFamily => WebConfigurationManager.AppSettings["Package_FontFamily"];
        public string FontColour => WebConfigurationManager.AppSettings["Package_FontColour"];
        public int BadgeHeight => int.Parse(WebConfigurationManager.AppSettings["Package_Badge_Height"]);
        public int BadgeCornerRadius => int.Parse(WebConfigurationManager.AppSettings["Package_Badge_CornerRadius"]);
        public string BackgroundColourTitle => WebConfigurationManager.AppSettings["Package_Badge_BackgroundColour_Title"];
        public string BackgroundColourVersion => WebConfigurationManager.AppSettings["Package_Badge_BackgroundColour_Version"];
        public string BackgroundColourDownloads => WebConfigurationManager.AppSettings["Package_Badge_BackgroundColour_Downloads"]; 
    }
}