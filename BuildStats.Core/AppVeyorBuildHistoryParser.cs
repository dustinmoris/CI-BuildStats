using System.Collections.Generic;

namespace BuildStats.Core
{
    public sealed class AppVeyorBuildHistoryParser : IBuildHistoryParser
    {
        private readonly ISerializer _serializer;

        public AppVeyorBuildHistoryParser(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public IList<Build> Parse(string content)
        {
            var buildHistory = _serializer.Deserialize(content);
            var builds = new List<Build>();

            foreach (var item in buildHistory.builds)
            {
                builds.Add(
                    new Build(
                        item.buildId.Value,
                        item.buildNumber.Value,
                        ConvertStatus(item.status.Value),
                        item.started != null ? item.started.Value : null,
                        item.finished != null ? item.finished.Value : null,
                        item.branch.Value));
            }

            return builds;
        }

        private static BuildStatus ConvertStatus(string status)
        {
            switch (status)
            {
                case "success": return BuildStatus.Success;
                case "failed": return BuildStatus.Failed;
                case "cancelled": return BuildStatus.Cancelled;
                case "queued": return BuildStatus.Pending;
                case "running": return BuildStatus.Pending;
                default: return BuildStatus.Unknown;
            }
        }
    }
}