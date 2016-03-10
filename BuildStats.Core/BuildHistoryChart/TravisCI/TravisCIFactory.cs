namespace BuildStats.Core.BuildHistoryChart.TravisCI
{
    public sealed class TravisCIFactory : BuildSystemFactory
    {
        public override IBuildHistoryParser CreateBuildHistoryParser()
        {
            return new TravisCIBuildHistoryParser(CreateSerializer());
        }

        public override IBuildHistoryClient CreateBuildHistoryClient()
        {
            return new TravisCIBuildHistoryClient(
                CreateRestfulApiClient(),
                CreateBuildHistoryParser());
        }
    }
}