namespace BuildStats.Core.Common
{
    public interface ISerializer
    {
        dynamic Deserialize(string content);
    }
}