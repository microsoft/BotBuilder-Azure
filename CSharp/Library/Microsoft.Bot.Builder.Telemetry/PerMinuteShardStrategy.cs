using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class PerMinuteShardStrategy : IShardStrategy
    {
        public string CurrentShardKey => $"{DateTime.UtcNow.Year:yyyy-MM-dd-HH-mm}";
    }
}