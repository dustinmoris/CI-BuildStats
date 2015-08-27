using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuildStats.Core
{
    public interface IAppVeyorApiClient
    {
        Task<IList<Build>> GetBuilds(string account, string project, int buildCount);
    }
}