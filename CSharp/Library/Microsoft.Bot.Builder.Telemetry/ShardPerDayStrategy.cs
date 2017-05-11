using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class ShardPerDayStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow:yyyy-MM-dd}";
    }
}