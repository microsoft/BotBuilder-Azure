using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry.Data;

namespace Microsoft.Bot.Builder.Telemetry.TextFileWriter
{
    public class TextFileTelemetryWriter : ITelemetryWriter
    {
        private static readonly ReaderWriterLockSlim ReaderWriterLockInstance = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ITelemetryOutputFormatter _outputFormatter;
        private readonly TextFileTelemetryWriterConfiguration _configuration;

        public TextFileTelemetryWriter(TextFileTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out _outputFormatter, nameof(formatter), formatter);

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

        private void DoPostLogActions()
        {
            //no-op
            //method left in place to make it easy to add behavior here later if/as needed
        }

        public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatServiceResult(serviceResultTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatIntent(intentTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatEntity(entityTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Requests))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatRequest(requestTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatResponse(responseTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatCounter(counterTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatMeasure(measureTelemetryData));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatException(exceptionTelemetryData));
                    DoPostLogActions();
                });
            }
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

        public async Task WriteEventAsync(string key, string value)
        {
            await WriteEventAsync(new Dictionary<string, string> { { key, value } });
        }

        public void SetContext(ITelemetryContext context)
        {
            _outputFormatter.SetContext(context);
        }

        public async Task WriteEventAsync(string key, double value)
        {
            await WriteEventAsync(new Dictionary<string, double> { { key, value } });
        }

        public async Task WriteEventAsync(Dictionary<string, double> metrics)
        {
            await WriteEventAsync(new Dictionary<string, string>(), metrics);
        }

        public async Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatEvent(properties, metrics));
                    DoPostLogActions();
                });
            }
        }
    }
}