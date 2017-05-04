using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Bot.Builder.Azure.Telemetry.BlobStorageWriter
{
    public class BlobStorageTelemetryWriter : ITelemetryWriter
    {
        private readonly ITelemetryOutputFormatter _formatter;
        private CloudAppendBlob _blob;
        private readonly BlobStorageTelemetryWriterConfiguration _configuration;

        public BlobStorageTelemetryWriter(BlobStorageTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out _formatter, nameof(formatter), formatter);

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

        private void DoPostLogActions()
        {
            //no-op; left to ease future use if needed
        }

        public async Task WriteIntentAsync(IntentTelemetry intentTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(async () =>
                {
                    //process each entity if we have any
                    if (null != intentTelemetry.Entities)
                    {
                        foreach (var entity in intentTelemetry.Entities)
                        {
                            await WriteEntityAsync(new EntityTelemetry(entity.Key, entity.Value));
                        }
                    }

                    //now process the intent
                    await AppendToBlob(_formatter.FormatIntent(new IntentTelemetry(intentTelemetry.Intent, intentTelemetry.Text, intentTelemetry.Score)));

                    DoPostLogActions();
                });
            }
        }

        private async Task AppendToBlob(string record)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(record)))
            {
                await _blob.AppendFromStreamAsync(stream);
            }
        }

        public async Task WriteEntityAsync(EntityTelemetry entityTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(async () =>
                {
                    await AppendToBlob(_formatter.FormatEntity(new EntityTelemetry(entityTelemetry.Kind, entityTelemetry.Value)));
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteResponseAsync(ResponseTelemetry responseTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                await Task.Run(async () =>
                {
                    await AppendToBlob(_formatter.FormatResponse(new ResponseTelemetry(responseTelemetry.Text, responseTelemetry.ImageUrl, responseTelemetry.Json, responseTelemetry.Result, responseTelemetry.StartTime, responseTelemetry.EndDateTime, responseTelemetry.IsCacheHit)));
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteServiceResultAsync(ServiceResultTelemetry serviceResultTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(async () =>
                {
                    await AppendToBlob(_formatter.FormatServiceResult(new ServiceResultTelemetry(new ServiceResultTelemetry(serviceResultTelemetry.ServiceName, serviceResultTelemetry.StartTime, serviceResultTelemetry.EndDateTime, serviceResultTelemetry.Result, serviceResultTelemetry.Success))));
                    DoPostLogActions();
                });
            }

        }

        public async Task WriteCounterAsync(CounterTelemetry counterTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(async () =>
                {
                    await AppendToBlob(_formatter.FormatCounter(new CounterTelemetry(counterTelemetry.Counter, counterTelemetry.Count)));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteExceptionAsync(ExceptionTelemetry exceptionTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(async () =>
                {
                    await AppendToBlob(_formatter.FormatException(new ExceptionTelemetry(exceptionTelemetry.Component, exceptionTelemetry.Context, exceptionTelemetry.E)));
                    DoPostLogActions();
                });
            }
        }

        public void SetContext(ITelemetryContext context)
        {
            _formatter.SetContext(context);
        }

        public async Task WriteEventAsync(string key, string value)
        {
            await WriteEventAsync(new Dictionary<string, string> { { key, value } });
        }

        public async Task WriteEventAsync(string key, double value)
        {
            await WriteEventAsync(new Dictionary<string, double> { { key, value } });
        }

        public async Task WriteEventAsync(Dictionary<string, double> metrics)
        {
            await WriteEventAsync(new Dictionary<string, string>(), metrics);
        }

        public async Task WriteEventAsync(Dictionary<string, string> eventProperties, Dictionary<string, double> eventMetrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                await Task.Run(async () =>
                {
                    await AppendToBlob(_formatter.FormatEvent(eventProperties, eventMetrics));
                    DoPostLogActions();
                });
            }
        }
    }
}