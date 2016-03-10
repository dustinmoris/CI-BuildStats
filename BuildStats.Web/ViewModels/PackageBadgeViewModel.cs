using System;
using System.Drawing;
using BuildStats.Core.PackageBadge;
using BuildStats.Web.Config;

namespace BuildStats.Web.ViewModels
{
    public sealed class PackageBadgeViewModel
    {
        public PackageBadgeViewModel(
            string provider,
            IPackageBadgeConfig config,
            PackageInfo packageInfo)
        {
            Provider = provider;
            Config = config;
            PackageInfo = packageInfo;
        }

        public string Provider { get; }
        public IPackageBadgeConfig Config { get; }
        public PackageInfo PackageInfo { get; }

        public string GetDownloadsText()
        {
            const int million = 1000000;
            const int thousand = 1000;
            var downloads = PackageInfo.Downloads;

            return downloads >= million
                ? $"{Math.Round((double)downloads / million, 2)}m"
                : (
                    downloads >= thousand
                        ? $"{Math.Round((double)downloads / thousand, 1)}k"
                        : downloads.ToString());
        }

        public int MeasureTextWidth(string text, int fontSize)
        {
            var bitmap = new Bitmap(1, 1);
            var graphics = Graphics.FromImage(bitmap);
            var font = new Font(FontFamily.GenericSansSerif, fontSize);
            var dimension = graphics.MeasureString(text, font);
            return (int)Math.Ceiling(dimension.Width);
        }
    }
}