using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.TextFileWriter
{
    public class TextFileTelemetryWriter : StringOutputTelemetryWriterBase, ITelemetryWriter
    {
        private static readonly ReaderWriterLockSlim ReaderWriterLockInstance = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly TextFileTelemetryWriterConfiguration _configuration;

        public TextFileTelemetryWriter(TextFileTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out OutputFormatter, nameof(formatter), formatter);

            _configuration.ValidateSettings();

            Initialize();
        }

        private void Initialize()
        {
            if (_configuration.OverwriteFileIfExists && File.Exists(_configuration.Filename))
            {
                File.Delete(_configuration.Filename);
            }
        }

        public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            await DoWriteTelemetry(serviceResultTelemetryData, TelemetryTypes.ServiceResults,
                OutputFormatter.FormatServiceResult);
        }

        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            await DoWriteTelemetry(intentTelemetryData, TelemetryTypes.Intents,
                OutputFormatter.FormatIntent);
        }

        public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            await DoWriteTelemetry(entityTelemetryData, TelemetryTypes.Entities,
                OutputFormatter.FormatEntity);
        }

        public async Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            await DoWriteTelemetry(requestTelemetryData, TelemetryTypes.Requests,
                OutputFormatter.FormatRequest);
        }

        public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            await DoWriteTelemetry(responseTelemetryData, TelemetryTypes.Responses,
                OutputFormatter.FormatResponse);
        }

        public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            await DoWriteTelemetry(counterTelemetryData, TelemetryTypes.Counters,
                OutputFormatter.FormatCounter);
        }

        public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            await DoWriteTelemetry(measureTelemetryData, TelemetryTypes.Measures,
                OutputFormatter.FormatMeasure);
        }

        public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            await DoWriteTelemetry(exceptionTelemetryData, TelemetryTypes.Exceptions,
                OutputFormatter.FormatException);
        }

        private void ThreadsafeWriteToFile(string message)
        {
            try
            {
                ReaderWriterLockInstance.TryEnterWriteLock(int.MaxValue);
                File.AppendAllText(_configuration.Filename, message);
            }
            finally
            {
                ReaderWriterLockInstance.ExitWriteLock();
            }
        }

        public override async Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(OutputFormatter.FormatEvent(properties, metrics));
                });
            }
        }

        protected override async Task DoWriteTelemetry<TTelemetryData>(TTelemetryData telemetryData, TelemetryTypes handleTypes, Func<TTelemetryData, string> formatter)
        {
            if (_configuration.Handles(handleTypes))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(formatter(telemetryData));
                });
            }
        }
    }
}