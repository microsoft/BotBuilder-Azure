namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public interface IShardStrategy
    {
        string CurrentShardKey { get; }
    }
}