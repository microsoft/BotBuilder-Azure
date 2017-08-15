using System;

namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public class ShardPerDayStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow:yyyy-MM-dd}";
    }
}