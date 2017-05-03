using Microsoft.Bot.Builder.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Telemetry.BlobStorageWriter
{
    public class DefaultContainerShardStrategy : IShardStrategy
    {
        public string CurrentShardKey => "telemetry-container";
    }
}