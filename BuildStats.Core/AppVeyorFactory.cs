namespace BuildStats.Core
{
    public sealed class AppVeyorFactory : IBuildSystemFactory
    {
        private readonly string _apiBuildHistoryUrlFormat;

        public AppVeyorFactory(string apiBuildHistoryUrlFormat)
        {
            _apiBuildHistoryUrlFormat = apiBuildHistoryUrlFormat;
        }

        public string GetApiBuildHistoryUrlFormat()
        {
            return _apiBuildHistoryUrlFormat;
        }

        public IBuildHistoryParser CreateBuildHistoryParser()
        {
            return new AppVeyorBuildHistoryParser();
        }
    }
}