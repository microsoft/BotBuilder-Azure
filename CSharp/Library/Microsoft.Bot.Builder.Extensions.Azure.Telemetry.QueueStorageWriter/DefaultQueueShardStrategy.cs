using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.QueueStorageWriter
{
    /// <summary>
    /// Class DefaultQueueShardStrategy.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.IShardStrategy" />
    public class DefaultQueueShardStrategy : IShardStrategy
    {
        /// <summary>
        /// Gets the current shard key.
        /// </summary>
        /// <value>The current shard key.</value>
        public string CurrentShardKey => "telemetry-queue";
    }
}