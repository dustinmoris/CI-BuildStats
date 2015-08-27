namespace BuildStats.Web.Config
{
    public interface IChartConfig
    {
        int DefaultBuildCount { get; }
        int FontSize { get; }
        int PaddingX { get; }
        int BarWidth { get; }
        int BarMaxHeight { get; }
        int BarGap { get; }
        string TimeSpanFormat { get; }
        string TextColorCode { get; }
        string TitleColorCode { get; }
        string SucceededColorCode { get; }
        string FailedColorCode { get; }
        string CancelledColorCode { get; }
    }
}