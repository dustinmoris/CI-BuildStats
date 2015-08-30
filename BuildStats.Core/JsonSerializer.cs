using Newtonsoft.Json;

namespace BuildStats.Core
{
    public sealed class JsonSerializer : ISerializer
    {
        public dynamic Deserialize(string content)
        {
            return JsonConvert.DeserializeObject<dynamic>(content);
        }
    }
}