namespace BuildStats.Core
{
    public interface IBuildSystemFactory
    {
        string GetApiBuildHistoryUrlFormat();
        IBuildHistoryParser CreateBuildHistoryParser();
    }
}