using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public interface IBuildHistoryClient
    {
        Task<IList<Build>> GetBuilds(string account, string project, string branch, int buildCount, bool includeBuildsFromPullRequest);
    }
}