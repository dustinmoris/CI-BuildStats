using System.Web.Configuration;

namespace BuildStats.Core
{
    public sealed class TravisCIFactory : BuildSystemFactory
    {
        public override string CreateBuildHistoryApiUrlFormat()
        {
            return WebConfigurationManager.AppSettings["TravisCI_API_BuildHistory_URL_Format"];
        }

        public override IBuildHistoryParser CreateBuildHistoryParser()
        {
            return new TravisCIBuildHistoryParser(CreateSerializer());
        }

        public override IBuildHistoryClient CreateBuildHistoryClient()
        {
            return new TravisCIBuildHistoryClient(
                CreateRestfulApiClient(),
                CreateBuildHistoryParser(),
                CreateBuildHistoryApiUrlFormat());
        }
    }
}