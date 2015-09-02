using System;

namespace BuildStats.Core
{
    public sealed class Build
    {
        public long BuildId { get; }
        public long BuildNumber { get; }
        public TimeSpan TotalTime { get; }
        public BuildStatus Status { get; }
        public string Branch { get; }
        public bool FromPullRequest { get; }

        public Build(
            long buildId,
            long buildNumber,
            BuildStatus status,
            DateTime? started,
            DateTime? finished,
            string branch,
            bool fromPullRequest)
        {
            BuildId = buildId;
            BuildNumber = buildNumber;
            Status = status;
            Branch = branch;
            FromPullRequest = fromPullRequest;

            TotalTime = started != null && finished != null
                ? finished.Value - started.Value 
                : TimeSpan.Zero;
        }
    }
}