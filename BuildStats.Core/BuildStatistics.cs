using System;
using System.Collections.Generic;
using System.Linq;

namespace BuildStats.Core
{
    public sealed class BuildStatistics : IBuildStatistics
    {
        public TimeSpan GetLongestBuildTime(ICollection<Build> builds)
        {
            if (builds == null || builds.Count == 0)
                return TimeSpan.Zero;

            return builds.Where(b => b.Status != BuildStatus.Cancelled).Max(b => b.TotalTime);
        }

        public TimeSpan GetShortestBuildTime(ICollection<Build> builds)
        {
            if (builds == null || builds.Count == 0)
                return TimeSpan.Zero;

            return builds.Where(b => b.Status != BuildStatus.Cancelled).Min(b => b.TotalTime);
        }

        public TimeSpan GetAverageBuildTime(ICollection<Build> builds)
        {
            if (builds == null || builds.Count == 0)
                return TimeSpan.Zero;

            return TimeSpan.FromMilliseconds(builds.Where(b => b.Status != BuildStatus.Cancelled).Average(b => b.TotalTime.TotalMilliseconds));
        }
    }
}