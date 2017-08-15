using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.BlobStorageWriter
{
    public class BlobStorageTelemetryWriter : StringOutputTelemetryWriterBase, ITelemetryWriter
    {
        private CloudAppendBlob _blob;
        private readonly BlobStorageTelemetryWriterConfiguration _configuration;

        public BlobStorageTelemetryWriter(BlobStorageTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out OutputFormatter, nameof(formatter), formatter);

            Initialize();
        }

        private void Initialize()
        {
            //set initial configuration
            _blob = GetAppendBlob();
        }

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

        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            await DoWriteTelemetry(intentTelemetryData, TelemetryTypes.Intents, OutputFormatter.FormatIntent);
            }

        private async Task AppendToBlob(string record)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(record)))
            {
                await _blob.AppendBlockAsync(stream);
            }
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
                    await AppendToBlob(OutputFormatter.FormatEvent(eventProperties, eventMetrics));
                });
            }
        }

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