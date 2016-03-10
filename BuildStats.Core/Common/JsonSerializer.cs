using Newtonsoft.Json;

namespace BuildStats.Core.Common
{
    public sealed class JsonSerializer : ISerializer
    {
        public dynamic Deserialize(string content)
        {
            return JsonConvert.DeserializeObject<dynamic>(content);
        }
    }
}