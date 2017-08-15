using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.BlobStorageWriter
{
    public class DefaultContainerShardStrategy : IShardStrategy
    {
        public string CurrentShardKey => "telemetry-container";
    }
}