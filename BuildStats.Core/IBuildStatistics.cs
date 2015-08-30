using System;
using System.Collections.Generic;

namespace BuildStats.Core
{
    public interface IBuildStatistics
    {
        TimeSpan GetLongestBuildTime(IEnumerable<Build> builds);
        TimeSpan GetShortestBuildTime(IEnumerable<Build> builds);
        TimeSpan GetAverageBuildTime(IEnumerable<Build> builds);
    }
}