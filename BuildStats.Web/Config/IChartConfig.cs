namespace BuildStats.Web.Config
{
    public interface IChartConfig
    {
        int DefaultBuildCount { get; }
        int FontSize { get; }
        int BarWidth { get; }
        int BarMaxHeight { get; }
        int BarGap { get; }
        string TimeSpanFormat { get; }
        string TextColorCode { get; }
        string TitleColorCode { get; }
        string SuccessColorCode { get; }
        string FailedColorCode { get; }
        string PendingColorCode { get; }
        string CancelledColorCode { get; }
    }
}