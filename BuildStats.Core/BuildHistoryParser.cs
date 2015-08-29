using System.Collections.Generic;
using Newtonsoft.Json;

namespace BuildStats.Core
{
    public sealed class BuildHistoryParser : IBuildHistoryParser
    {
        public IList<Build> ParseJson(string jsonContent)
        {
            var buildHistory = DeserializeJson(jsonContent);
            var builds = new List<Build>();

            foreach (var item in buildHistory.builds)
            {
                builds.Add(
                    new Build(
                        item.buildId.Value.ToString(),
                        item.buildNumber.Value.ToString(),
                        item.version.Value,
                        item.started != null ? item.started.Value : null,
                        item.finished != null ? item.finished.Value : null,
                        DeserializeStatus(item.status.Value)));
            }

            return builds;
        }

        private dynamic DeserializeJson(string jsonContent)
        {
            return JsonConvert.DeserializeObject<dynamic>(jsonContent);
        }

        private BuildStatus DeserializeStatus(string status)
        {
            switch (status)
            {
                case "success": return BuildStatus.Success;
                case "failed": return BuildStatus.Failed;
                case "cancelled": return BuildStatus.Cancelled;
                case "queued": return BuildStatus.Queued;
                case "running": return BuildStatus.Running;
                default:return BuildStatus.Unkown;
            }
        }
    }
}