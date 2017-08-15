using System;
using Microsoft.Azure;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.BlobStorageWriter
{
    public class BlobStorageTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        private readonly IShardStrategy _containerShardStrategy;
        private readonly IShardStrategy _blobShardStrategy;

        public BlobStorageTelemetryWriterConfiguration(IShardStrategy containerShardStrategy = null, IShardStrategy blobShardStrategy = null)
        {
            if (null == containerShardStrategy)
            {
                containerShardStrategy = new DefaultContainerShardStrategy();
            }

            if (null == blobShardStrategy)
            {
                blobShardStrategy = new ShardPerDayStrategy();
            }

            SetField.NotNull(out _containerShardStrategy, nameof(containerShardStrategy), containerShardStrategy);
            SetField.NotNull(out _blobShardStrategy, nameof(blobShardStrategy), blobShardStrategy);

            try
            {
                StorageConnectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            }
            catch (Exception)
            {
                //because this is an attempt to read from a well-known location it may fail
                // so swallow all errors vs. throwing from the ctor
            }
        }

        public string StorageConnectionString { get; set; }
        public string BlobStorageContainerName => _containerShardStrategy.CurrentShardKey;
        public string BlobStorageBlobName => $"{_blobShardStrategy.CurrentShardKey}-telemetry.txt";
    }
}