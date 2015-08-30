using WebConfig = System.Web.Configuration.WebConfigurationManager;

namespace BuildStats.Web.Config
{
    public sealed class ChartConfig : IChartConfig
    {
        public ChartConfig()
        {
            DefaultBuildCount = int.Parse(WebConfig.AppSettings["Default_Build_Count"]);
            FontSize = int.Parse(WebConfig.AppSettings["Chart_FontSize"]);
            BarWidth = int.Parse(WebConfig.AppSettings["Chart_Bar_Width"]);
            BarMaxHeight = int.Parse(WebConfig.AppSettings["Chart_Bar_MaxHeight"]);
            BarGap = int.Parse(WebConfig.AppSettings["Chart_Bar_Gap"]);
            TimeSpanFormat = WebConfig.AppSettings["Chart_TimeSpan_Format"];
            TextColorCode = WebConfig.AppSettings["Chart_Text_Color_Code"];
            TitleColorCode = WebConfig.AppSettings["Chart_Title_Color_Code"];
            SuccessColorCode = WebConfig.AppSettings["Chart_Success_Color_Code"];
            FailedColorCode = WebConfig.AppSettings["Chart_Failed_Color_Code"];
            PendingColorCode = WebConfig.AppSettings["Chart_Pending_Color_Code"];
            CancelledColorCode = WebConfig.AppSettings["Chart_Cancelled_Color_Code"];
        }

        public int DefaultBuildCount { get; }
        public int FontSize { get; }
        public int BarWidth { get; }
        public int BarMaxHeight { get; }
        public int BarGap { get; }
        public string TimeSpanFormat { get; }
        public string TextColorCode { get; }
        public string TitleColorCode { get; }
        public string SuccessColorCode { get; }
        public string FailedColorCode { get; }
        public string PendingColorCode { get; }
        public string CancelledColorCode { get; }
    }
}