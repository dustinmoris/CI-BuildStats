using System.Collections.Generic;
using BuildStats.Core.Common;

namespace BuildStats.Core.BuildHistoryChart.CircleCI
{
    public sealed class CircleCIBuildHistoryParser : IBuildHistoryParser
    {
        private readonly ISerializer _serializer;

        public CircleCIBuildHistoryParser(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public IList<Build> Parse(string content)
        {
            var buildHistory = _serializer.Deserialize(content);
            var builds = new List<Build>();

            foreach (var item in buildHistory)
            {
                builds.Add(
                    new Build(
                        item.build_num.Value,
                        int.Parse(item.build_num.Value.ToString()),
                        ConvertStatus(item.status.Value),
                        item.start_time != null ? item.start_time.Value : null,
                        item.stop_time != null ? item.stop_time.Value : null,
                        item.branch.Value,
                        item.subject !=  null && item.subject.Value.ToString().ToLower().Contains("pull request")));
            }

            return builds;
        }

        private static BuildStatus ConvertStatus(string status)
        {
            switch (status)
            {
                case "success": return BuildStatus.Success;
                case "fixed": return BuildStatus.Success;
                case "no_tests": return BuildStatus.Success;
                case "failed": return BuildStatus.Failed;
                case "infrastructure_fail": return BuildStatus.Failed;
                case "timedout": return BuildStatus.Failed;
                case "canceled": return BuildStatus.Cancelled;
                case "not_run": return BuildStatus.Cancelled;
                case "not_running": return BuildStatus.Cancelled;
                case "scheduled": return BuildStatus.Pending;
                case "queued": return BuildStatus.Pending;
                case "running": return BuildStatus.Pending;
                default: return BuildStatus.Unknown;
            }
        }
    }
}