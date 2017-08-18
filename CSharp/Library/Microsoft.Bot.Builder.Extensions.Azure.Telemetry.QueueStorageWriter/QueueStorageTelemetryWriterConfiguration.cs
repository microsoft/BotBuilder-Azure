using System;
using Microsoft.Azure;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.QueueStorageWriter
{
    /// <summary>
    /// Class QueueStorageTelemetryWriterConfiguration.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.TypeDiscriminatingTelemetryWriterConfigurationBase" />
    public class QueueStorageTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        /// <summary>
        /// The queue shard strategy
        /// </summary>
        private readonly IShardStrategy _queueShardStrategy;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueStorageTelemetryWriterConfiguration"/> class.
        /// </summary>
        /// <param name="queueShardStrategy">The queue shard strategy.</param>
        public QueueStorageTelemetryWriterConfiguration(IShardStrategy queueShardStrategy = null)
        {
            if (null == queueShardStrategy)
            {
                queueShardStrategy = new DefaultQueueShardStrategy();
            }

            SetField.NotNull(out _queueShardStrategy, nameof(queueShardStrategy), queueShardStrategy);

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
        /// Gets the name of the queue storage queue.
        /// </summary>
        /// <value>The name of the queue storage queue.</value>
        public string QueueStorageQueueName => $"{_queueShardStrategy.CurrentShardKey}-telemetry";
    }
}