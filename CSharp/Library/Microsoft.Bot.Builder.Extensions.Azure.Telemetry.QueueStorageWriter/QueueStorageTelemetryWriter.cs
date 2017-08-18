using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.QueueStorageWriter
{
    /// <summary>
    /// Class QueueStorageTelemetryWriter.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.StringOutputTelemetryWriterBase" />
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.ITelemetryWriter" />
    public class QueueStorageTelemetryWriter : StringOutputTelemetryWriterBase, ITelemetryWriter
    {
        /// <summary>
        /// The queue
        /// </summary>
        private CloudQueue _queue;
        
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly QueueStorageTelemetryWriterConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueStorageTelemetryWriter"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="formatter">The formatter.</param>
        public QueueStorageTelemetryWriter(QueueStorageTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out OutputFormatter, nameof(formatter), formatter);

            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            //set initial configuration
            _queue = GetStorageQueue();
        }

        /// <summary>
        /// Gets the storage queue.
        /// </summary>
        /// <returns>CloudQueue.</returns>
        private CloudQueue GetStorageQueue()
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

        /// <summary>
        /// write intent as an asynchronous operation.
        /// </summary>
        /// <param name="intentTelemetryData">The intent telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            await DoWriteTelemetry(intentTelemetryData, TelemetryTypes.Intents, OutputFormatter.FormatIntent);
        }

        /// <summary>
        /// Enqueues the message.
        /// </summary>
        /// <param name="telemetryRecord">The telemetry record.</param>
        /// <returns>Task.</returns>
        private async Task EnqueueMessage(string telemetryRecord)
        {
            await _queue.AddMessageAsync(new CloudQueueMessage(telemetryRecord));
        }

        /// <summary>
        /// write entity as an asynchronous operation.
        /// </summary>
        /// <param name="entityTelemetryData">The entity telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            await DoWriteTelemetry(entityTelemetryData, TelemetryTypes.Entities, OutputFormatter.FormatEntity);
        }

        /// <summary>
        /// write request as an asynchronous operation.
        /// </summary>
        /// <param name="requestTelemetryData">The request telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            await DoWriteTelemetry(requestTelemetryData, TelemetryTypes.Exceptions, OutputFormatter.FormatRequest);
        }

        /// <summary>
        /// write response as an asynchronous operation.
        /// </summary>
        /// <param name="responseTelemetryData">The response telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            await DoWriteTelemetry(responseTelemetryData, TelemetryTypes.Responses, OutputFormatter.FormatResponse);
        }

        /// <summary>
        /// write service result as an asynchronous operation.
        /// </summary>
        /// <param name="serviceResultTelemetryData">The service result telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            await DoWriteTelemetry(serviceResultTelemetryData, TelemetryTypes.ServiceResults, OutputFormatter.FormatServiceResult);
        }

        /// <summary>
        /// write counter as an asynchronous operation.
        /// </summary>
        /// <param name="counterTelemetryData">The counter telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            await DoWriteTelemetry(counterTelemetryData, TelemetryTypes.Counters, OutputFormatter.FormatCounter);
        }

        /// <summary>
        /// write measure as an asynchronous operation.
        /// </summary>
        /// <param name="measureTelemetryData">The measure telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            await DoWriteTelemetry(measureTelemetryData, TelemetryTypes.Measures, OutputFormatter.FormatMeasure);
        }

        /// <summary>
        /// write exception as an asynchronous operation.
        /// </summary>
        /// <param name="exceptionTelemetryData">The exception telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            await DoWriteTelemetry(exceptionTelemetryData, TelemetryTypes.Exceptions, OutputFormatter.FormatException);
        }

        /// <summary>
        /// write event as an asynchronous operation.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="eventMetrics">The event metrics.</param>
        /// <returns>Task.</returns>
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

        /// <summary>
        /// Actually write the telemetry.
        /// </summary>
        /// <typeparam name="TTelemetryData">The type of the telemetry data.</typeparam>
        /// <param name="telemetryData">The telemetry data.</param>
        /// <param name="handleTypes">The types of telemetry to handle.</param>
        /// <param name="formatter">The output formatter.</param>
        /// <returns>Task.</returns>
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