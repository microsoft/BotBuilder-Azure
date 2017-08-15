using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.QueueStorageWriter
{
    public class QueueStorageTelemetryWriter : StringOutputTelemetryWriterBase, ITelemetryWriter
    {
        private CloudQueue _queue;
        private readonly QueueStorageTelemetryWriterConfiguration _configuration;

        public QueueStorageTelemetryWriter(QueueStorageTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out OutputFormatter, nameof(formatter), formatter);

            Initialize();
        }

        private void Initialize()
        {
            //set initial configuration
            _queue = GetStorargeQueue();
        }

        private CloudQueue GetStorargeQueue()
        {
            //Parse the connection string for the storage account.
            var storageAccount = CloudStorageAccount.Parse(_configuration.StorageConnectionString);

            //Create service client for credentialed access to the Blob service.
            var queueClient = storageAccount.CreateCloudQueueClient();

            //Get a reference to a queue.
            var queue = queueClient.GetQueueReference(_configuration.QueueStorageQueueName);

            //Create the queue if it does not already exist.
            queue.CreateIfNotExists();

            return queue;
        }

        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            await DoWriteTelemetry(intentTelemetryData, TelemetryTypes.Intents, OutputFormatter.FormatIntent);
        }

        private async Task EnqueueMessage(string telemetryRecord)
        {
            await _queue.AddMessageAsync(new CloudQueueMessage(telemetryRecord));
        }

        public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            await DoWriteTelemetry(entityTelemetryData, TelemetryTypes.Entities, OutputFormatter.FormatEntity);
        }

        public async Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            await DoWriteTelemetry(requestTelemetryData, TelemetryTypes.Exceptions, OutputFormatter.FormatRequest);
        }

        public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            await DoWriteTelemetry(responseTelemetryData, TelemetryTypes.Responses, OutputFormatter.FormatResponse);
        }

        public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            await DoWriteTelemetry(serviceResultTelemetryData, TelemetryTypes.ServiceResults, OutputFormatter.FormatServiceResult);
        }

        public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            await DoWriteTelemetry(counterTelemetryData, TelemetryTypes.Counters, OutputFormatter.FormatCounter);
        }

        public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            await DoWriteTelemetry(measureTelemetryData, TelemetryTypes.Measures, OutputFormatter.FormatMeasure);
        }

        public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            await DoWriteTelemetry(exceptionTelemetryData, TelemetryTypes.Exceptions, OutputFormatter.FormatException);
        }

        public override async Task WriteEventAsync(Dictionary<string, string> eventProperties, Dictionary<string, double> eventMetrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                await Task.Run(async () =>
                {
                    await EnqueueMessage(OutputFormatter.FormatEvent(eventProperties, eventMetrics));
                });
            }
        }

        protected override async Task DoWriteTelemetry<TTelemetryData>(TTelemetryData telemetryData, TelemetryTypes handleTypes, Func<TTelemetryData, string> formatter)
        {
            if (_configuration.Handles(handleTypes))
            {
                await Task.Run(async () =>
                {
                    await EnqueueMessage(formatter(telemetryData));
                });
            }
        }
    }
}