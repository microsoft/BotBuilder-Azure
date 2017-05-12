using Microsoft.Bot.Builder.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Telemetry.QueueStorageWriter
{
    public class DefaultQueueShardStrategy : IShardStrategy
    {
        public string CurrentShardKey => "telemetry-queue";
    }
}