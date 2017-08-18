using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.BlobStorageWriter
{
    /// <summary>
    /// Class DefaultContainerShardStrategy.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.IShardStrategy" />
    public class DefaultContainerShardStrategy : IShardStrategy
    {
        /// <summary>
        /// Gets the current shard key.
        /// </summary>
        /// <value>The current shard key.</value>
        public string CurrentShardKey => "telemetry-container";
    }
}