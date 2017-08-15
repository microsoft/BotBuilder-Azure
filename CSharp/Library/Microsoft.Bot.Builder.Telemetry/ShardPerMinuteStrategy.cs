using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class ShardPerMinuteStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
    }
}