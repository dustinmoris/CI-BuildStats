using WebConfig = System.Web.Configuration.WebConfigurationManager;

namespace BuildStats.Web.Config
{
    public sealed class ChartConfig : IChartConfig
    {
        public int DefaultBuildCount => int.Parse(WebConfig.AppSettings["Default_Build_Count"]);
        public int FontSize => int.Parse(WebConfig.AppSettings["Chart_FontSize"]);
        public int BarWidth => int.Parse(WebConfig.AppSettings["Chart_Bar_Width"]);
        public int BarMaxHeight => int.Parse(WebConfig.AppSettings["Chart_Bar_MaxHeight"]);
        public int BarGap => int.Parse(WebConfig.AppSettings["Chart_Bar_Gap"]);
        public string TimeSpanFormat => WebConfig.AppSettings["Chart_TimeSpan_Format"];
        public string TextColorCode => WebConfig.AppSettings["Chart_Text_Color_Code"];
        public string TitleColorCode => WebConfig.AppSettings["Chart_Title_Color_Code"];
        public string SuccessColorCode => WebConfig.AppSettings["Chart_Success_Color_Code"];
        public string FailedColorCode => WebConfig.AppSettings["Chart_Failed_Color_Code"];
        public string PendingColorCode => WebConfig.AppSettings["Chart_Pending_Color_Code"];
        public string CancelledColorCode => WebConfig.AppSettings["Chart_Cancelled_Color_Code"];
    }
}