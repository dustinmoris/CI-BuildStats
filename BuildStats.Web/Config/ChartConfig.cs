namespace BuildStats.Web.Config
{
    public class ChartConfig : IChartConfig
    {
        public ChartConfig()
        {
            DefaultBuildCount = AppConfig.Default_Build_Count;
            FontSize = AppConfig.Chart_FontSize;
            PaddingX = AppConfig.Chart_Padding_X;
            BarWidth = AppConfig.Chart_Bar_Width;
            BarMaxHeight = AppConfig.Chart_Bar_MaxHeight;
            BarGap = AppConfig.Chart_Bar_Gap;
            TimeSpanFormat = AppConfig.Chart_TimeSpan_Format;
            TextColorCode = AppConfig.Chart_Text_Color_Code;
            TitleColorCode = AppConfig.Chart_Title_Color_Code;
            SucceededColorCode = AppConfig.Chart_Succeeded_Color_Code;
            FailedColorCode = AppConfig.Chart_Failed_Color_Code;
            CancelledColorCode = AppConfig.Chart_Cancelled_Color_Code;
        }

        public int DefaultBuildCount { get; }
        public int FontSize { get; }
        public int PaddingX { get; }
        public int BarWidth { get; }
        public int BarMaxHeight { get; }
        public int BarGap { get; }
        public string TimeSpanFormat { get; }
        public string TextColorCode { get; }
        public string TitleColorCode { get; }
        public string SucceededColorCode { get; }
        public string FailedColorCode { get; }
        public string CancelledColorCode { get; }
    }
}