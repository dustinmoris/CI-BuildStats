using WebConfig = System.Web.Configuration.WebConfigurationManager;

namespace BuildStats.Web
{
    public sealed class AppConfig
    {
        public static string AppVeyor_API_BuildHistory_URL_Format = WebConfig.AppSettings["AppVeyor_API_BuildHistory_URL_Format"];
        public static int Default_Build_Count = int.Parse(WebConfig.AppSettings["Default_Build_Count"]);
        public static int Chart_FontSize = int.Parse(WebConfig.AppSettings["Chart_FontSize"]);
        public static int Chart_Padding_X = int.Parse(WebConfig.AppSettings["Chart_Padding_X"]);
        public static int Chart_Padding_Y_Multiplier = int.Parse(WebConfig.AppSettings["Chart_Padding_Y_Multiplier"]);
        public static int Chart_Bar_MaxHeight = int.Parse(WebConfig.AppSettings["Chart_Bar_MaxHeight"]);
        public static int Chart_Bar_Width = int.Parse(WebConfig.AppSettings["Chart_Bar_Width"]);
        public static int Chart_Bar_Gap = int.Parse(WebConfig.AppSettings["Chart_Bar_Gap"]);
        public static string Chart_Failed_Color_Code = WebConfig.AppSettings["Chart_Failed_Color_Code"];
        public static string Chart_Succeeded_Color_Code = WebConfig.AppSettings["Chart_Succeeded_Color_Code"];
        public static string Chart_Cancelled_Color_Code = WebConfig.AppSettings["Chart_Cancelled_Color_Code"];
        public static string Chart_Title_Color_Code = WebConfig.AppSettings["Chart_Title_Color_Code"];
        public static string Chart_Text_Color_Code = WebConfig.AppSettings["Chart_Text_Color_Code"];
        public static string Chart_TimeSpan_Format = WebConfig.AppSettings["Chart_TimeSpan_Format"];
    }
}