namespace BuildStats.Web.Config
{
    public interface IPackageBadgeConfig
    {
        int FontSize { get; }
        string FontFamily { get; }
        string FontColour { get; }
        int BadgeHeight { get; }
        int BadgeCornerRadius { get; }
        string BackgroundColourTitle { get; }
        string BackgroundColourVersion { get; }
        string BackgroundColourDownloads { get; }
    }
}