namespace BuildStats.Core
{
    public sealed class AppVeyorFactory : BuildSystemFactory
    {
        public override IBuildHistoryParser CreateBuildHistoryParser()
        {
            return new AppVeyorBuildHistoryParser(CreateSerializer());
        }

        public override IBuildHistoryClient CreateBuildHistoryClient()
        {
            return new AppVeyorBuildHistoryClient(
                CreateRestfulApiClient(),
                CreateBuildHistoryParser());
        }
    }
}