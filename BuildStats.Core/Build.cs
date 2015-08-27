using System;

namespace BuildStats.Core
{
    public sealed class Build
    {
        public string BuildId { get; }
        public string BuildNumber { get; }
        public string Version { get; }
        public DateTime? Started { get; }
        public DateTime? Finished { get; }
        public TimeSpan TotalTime { get; }
        public BuildStatus Status { get; set; }

        public Build(
            string buildId, 
            string buildNumber, 
            string version, 
            DateTime? started, 
            DateTime? finished,
            BuildStatus status)
        {
            BuildId = buildId;
            BuildNumber = buildNumber;
            Version = version;
            Started = started;
            Finished = finished;
            Status = status;

            TotalTime = Started != null && Finished != null
                ? Finished.Value - Started.Value 
                : TimeSpan.Zero;
        }
    }
}