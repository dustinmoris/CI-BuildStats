namespace BuildStats.Core.BuildHistoryChart.CircleCI
{
    public sealed class CircleCIFactory : BuildSystemFactory
    {
        public override IBuildHistoryParser CreateBuildHistoryParser()
        {
            return new CircleCIBuildHistoryParser(CreateSerializer());
        }

        public override IBuildHistoryClient CreateBuildHistoryClient()
        {
            return new CircleCIBuildHistoryClient(
                CreateRestfulApiClient(),
                CreateBuildHistoryParser());
        }
    }
}