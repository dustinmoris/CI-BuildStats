namespace BuildStats.Core
{
    public abstract class BuildSystemFactory
    {
        protected virtual ISerializer CreateSerializer()
        {
            return new JsonSerializer();
        }

        protected virtual IRestfulApiClient CreateRestfulApiClient()
        {
            return new RestfulApiClient();
        }

        public abstract string CreateBuildHistoryApiUrlFormat();

        public abstract IBuildHistoryParser CreateBuildHistoryParser();

        public abstract IBuildHistoryClient CreateBuildHistoryClient();
    }
}