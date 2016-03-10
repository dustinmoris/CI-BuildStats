using System.Collections.Generic;

namespace BuildStats.Core.BuildHistoryChart
{
    public interface IBuildHistoryParser
    {
        IList<Build> Parse(string content);
    }
}