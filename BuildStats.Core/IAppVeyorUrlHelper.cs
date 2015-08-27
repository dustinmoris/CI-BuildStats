namespace BuildStats.Core
{
    public interface IAppVeyorUrlHelper
    {
        string CreateBuildHistoryUrl(string account, string project, int buildCount);
    }
}