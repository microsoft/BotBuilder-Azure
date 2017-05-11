using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class ShardPerMinuteStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow.Year:yyyy-MM-dd-HH-mm}";
    }
}