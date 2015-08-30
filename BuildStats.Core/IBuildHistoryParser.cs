using System.Collections.Generic;

namespace BuildStats.Core
{
    public interface IBuildHistoryParser
    {
        IList<Build> Parse(string content);
    }
}