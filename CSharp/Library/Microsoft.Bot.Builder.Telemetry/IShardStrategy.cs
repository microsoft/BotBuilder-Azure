namespace Microsoft.Bot.Builder.Telemetry
{
    public interface IShardStrategy
    {
        string CurrentShardKey { get; }
    }
}