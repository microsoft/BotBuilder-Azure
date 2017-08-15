using System;

namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public class ShardPerMinuteStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow:yyyy-MM-dd-HH-mm}";
    }
}