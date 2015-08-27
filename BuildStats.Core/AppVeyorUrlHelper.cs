namespace BuildStats.Core
{
    public sealed class AppVeyorUrlHelper : IAppVeyorUrlHelper
    {
        private readonly string _appVeyorApiBuildHistoryUrlFormat;

        public AppVeyorUrlHelper(string appVeyorApiBuildHistoryUrlFormat)
        {
            _appVeyorApiBuildHistoryUrlFormat = appVeyorApiBuildHistoryUrlFormat;
        }

        public string CreateBuildHistoryUrl(string account, string project, int buildCount)
        {
            return string.Format(_appVeyorApiBuildHistoryUrlFormat, account, project, buildCount);
        }
    }
}
