using System;
using System.Collections.Generic;

namespace BuildStats.Core
{
    public interface IBuildStatistics
    {
        TimeSpan GetLongestBuildTime(IList<Build> builds);
        TimeSpan GetShortestBuildTime(IList<Build> builds);
        TimeSpan GetAverageBuildTime(IList<Build> builds);
    }
}