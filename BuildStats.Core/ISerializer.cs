namespace BuildStats.Core
{
    public interface ISerializer
    {
        dynamic Deserialize(string content);
    }
}