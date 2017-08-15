using System;
using Microsoft.Azure;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.QueueStorageWriter
{
    public class QueueStorageTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        private readonly IShardStrategy _queueShardStrategy;

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

        public string StorageConnectionString { get; set; }
        public string QueueStorageQueueName => $"{_queueShardStrategy.CurrentShardKey}-telemetry";
    }
}