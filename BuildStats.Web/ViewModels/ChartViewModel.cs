using System;
using System.Collections.Generic;
using BuildStats.Core.BuildHistoryChart;
using BuildStats.Web.Config;

namespace BuildStats.Web.ViewModels
{
    public sealed class ChartViewModel
    {
        public ChartViewModel(
            IChartConfig config,
            IList<Build> builds, 
            TimeSpan longestBuildtime, 
            TimeSpan shortestBuildTime, 
            TimeSpan averageBuildTime,
            bool showStats)
        {
            Config = config;
            Builds = builds;
            LongestBuildTime = longestBuildtime;
            ShortestBuildTime = shortestBuildTime;
            AverageBuildTime = averageBuildTime;
            ShowStats = showStats;
        }

        public IChartConfig Config { get; }
        public IList<Build> Builds { get; }
        public TimeSpan LongestBuildTime { get; }
        public TimeSpan ShortestBuildTime { get; }
        public TimeSpan AverageBuildTime { get; }
        public bool ShowStats { get; }
        public string Branch { get; set; }
    }
}