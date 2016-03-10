namespace BuildStats.Core.PackageBadge
{
    public sealed class PackageInfo
    {
        public PackageInfo(
            string name,
            string version,
            int downloads)
        {
            Name = name;
            Version = version;
            Downloads = downloads;
        }

        public string Name { get; }
        public string Version { get; }
        public int Downloads { get; }
    }
}