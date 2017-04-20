using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class PerDayShardStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow:yyyy-MM-dd}";
    }
}