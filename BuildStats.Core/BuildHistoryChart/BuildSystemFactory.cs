using BuildStats.Core.Common;

namespace BuildStats.Core.BuildHistoryChart
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

        public abstract IBuildHistoryParser CreateBuildHistoryParser();

        public abstract IBuildHistoryClient CreateBuildHistoryClient();
    }
}