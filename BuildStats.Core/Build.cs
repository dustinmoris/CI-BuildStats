using System;

namespace BuildStats.Core
{
    public sealed class Build
    {
        public long BuildId { get; }
        public long BuildNumber { get; }
        public TimeSpan TotalTime { get; }
        public BuildStatus Status { get; }

        public Build(
            long buildId,
            long buildNumber,
            BuildStatus status,
            DateTime? started,
            DateTime? finished)
        {
            BuildId = buildId;
            BuildNumber = buildNumber;
            Status = status;

            TotalTime = started != null && finished != null
                ? finished.Value - started.Value 
                : TimeSpan.Zero;
        }
    }
}