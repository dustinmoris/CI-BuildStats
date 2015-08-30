using System;
using System.Collections.Generic;

namespace BuildStats.Core
{
    public sealed class TravisCIBuildHistoryParser : IBuildHistoryParser
    {
        private readonly ISerializer _serializer;

        public TravisCIBuildHistoryParser(ISerializer serializer)
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
                        item.id.Value,
                        int.Parse(item.number.Value),
                        ConvertStatus(item.state.Value, item.result.Value),
                        item.started_at != null ? item.started_at.Value : null,
                        item.finished_at != null ? item.finished_at.Value : null));
            }

            return builds;
        }

        private static BuildStatus ConvertStatus(string state, long? result)
        {
            switch (state)
            {
                case "finished": return result != null && result == 0 ? BuildStatus.Success : BuildStatus.Failed;
                case "started": return BuildStatus.Pending;
                default: return BuildStatus.Unknown;
            }
        }
    }
}