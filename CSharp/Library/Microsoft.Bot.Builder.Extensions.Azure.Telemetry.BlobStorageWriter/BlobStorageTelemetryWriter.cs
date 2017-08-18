using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.BlobStorageWriter
{
    /// <summary>
    /// Class BlobStorageTelemetryWriter.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.StringOutputTelemetryWriterBase" />
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.ITelemetryWriter" />
    public class BlobStorageTelemetryWriter : StringOutputTelemetryWriterBase, ITelemetryWriter
    {
        /// <summary>
        /// The BLOB
        /// </summary>
        private CloudAppendBlob _blob;
        
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly BlobStorageTelemetryWriterConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStorageTelemetryWriter"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="formatter">The formatter.</param>
        public BlobStorageTelemetryWriter(BlobStorageTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
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
            _blob = GetAppendBlob();
        }

        /// <summary>
        /// Gets the append BLOB.
        /// </summary>
        /// <returns>CloudAppendBlob.</returns>
        private CloudAppendBlob GetAppendBlob()
        {
            //Parse the connection string for the storage account.
            var storageAccount = CloudStorageAccount.Parse(_configuration.StorageConnectionString);

            //Create service client for credentialed access to the Blob service.
            var blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container.
            var container = blobClient.GetContainerReference(_configuration.BlobStorageContainerName);

            //Create the container if it does not already exist.
            container.CreateIfNotExists();

            //Get a reference to an append blob.
            var appendBlob = container.GetAppendBlobReference(_configuration.BlobStorageBlobName);

            //Create the append blob if not exists
            if (!appendBlob.Exists())
            {
                appendBlob.CreateOrReplace();
            }

            return appendBlob;
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
        /// Appends to BLOB.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <returns>Task.</returns>
        private async Task AppendToBlob(string record)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(record)))
            {
                await _blob.AppendBlockAsync(stream);
            }
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
                    await AppendToBlob(OutputFormatter.FormatEvent(eventProperties, eventMetrics));
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
                    await AppendToBlob(formatter(telemetryData));
                });
            }
        }
    }
}