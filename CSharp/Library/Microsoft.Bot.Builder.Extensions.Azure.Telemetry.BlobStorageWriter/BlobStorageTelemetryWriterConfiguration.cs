using System;
using Microsoft.Azure;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.BlobStorageWriter
{
    /// <summary>
    /// Class BlobStorageTelemetryWriterConfiguration.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.TypeDiscriminatingTelemetryWriterConfigurationBase" />
    public class BlobStorageTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        /// <summary>
        /// The container shard strategy
        /// </summary>
        private readonly IShardStrategy _containerShardStrategy;
        
        /// <summary>
        /// The BLOB shard strategy
        /// </summary>
        private readonly IShardStrategy _blobShardStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageTelemetryWriterConfiguration"/> class.
        /// </summary>
        /// <param name="containerShardStrategy">The container shard strategy.</param>
        /// <param name="blobShardStrategy">The BLOB shard strategy.</param>
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

        /// <summary>
        /// Gets or sets the storage connection string.
        /// </summary>
        /// <value>The storage connection string.</value>
        public string StorageConnectionString { get; set; }
        
        /// <summary>
        /// Gets the name of the BLOB storage container.
        /// </summary>
        /// <value>The name of the BLOB storage container.</value>
        public string BlobStorageContainerName => _containerShardStrategy.CurrentShardKey;
        
        /// <summary>
        /// Gets the name of the BLOB storage BLOB.
        /// </summary>
        /// <value>The name of the BLOB storage BLOB.</value>
        public string BlobStorageBlobName => $"{_blobShardStrategy.CurrentShardKey}-telemetry.txt";
    }
}