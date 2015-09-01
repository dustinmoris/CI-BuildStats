using System;
using System.Collections.Generic;

namespace BuildStats.Core
{
    public interface IBuildStatistics
    {
        TimeSpan GetLongestBuildTime(ICollection<Build> builds);
        TimeSpan GetShortestBuildTime(ICollection<Build> builds);
        TimeSpan GetAverageBuildTime(ICollection<Build> builds);
    }
}