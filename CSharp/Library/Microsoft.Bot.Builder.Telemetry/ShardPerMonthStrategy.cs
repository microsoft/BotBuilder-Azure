using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class ShardPerMonthStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow:yyyy-MM}";
    }
}