using System.Collections.Generic;

namespace BuildStats.Core
{
    public interface IBuildHistoryParser
    {
        IList<Build> ParseJson(string jsonContent);
    }
}