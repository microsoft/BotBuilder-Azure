using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.QueueStorageWriter
{
    public class DefaultQueueShardStrategy : IShardStrategy
    {
        public string CurrentShardKey => "telemetry-queue";
    }
}