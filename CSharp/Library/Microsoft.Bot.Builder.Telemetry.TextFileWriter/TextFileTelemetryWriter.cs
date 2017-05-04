using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

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
            //no-op
        }

        private void DoPostLogActions()
        {
            //no-op
            //method left in place to make it easy to add behavior here later if/as needed
        }

        public async Task WriteServiceResultAsync(ServiceResultTelemetry serviceResultTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatServiceResult(serviceResultTelemetry));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteIntentAsync(IntentTelemetry intentTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(async () =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatIntent(intentTelemetry));

                    if (null != intentTelemetry.Entities)
                    {
                        foreach (var entity in intentTelemetry.Entities)
                        {
                            await WriteEntityAsync(new EntityTelemetry(entity.Key, entity.Value));
                        }
                    }


                    DoPostLogActions();
                });
            }
        }

        public async Task WriteEntityAsync(EntityTelemetry entityTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatEntity(entityTelemetry));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteResponseAsync(ResponseTelemetry responseTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatResponse(responseTelemetry));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteCounterAsync(CounterTelemetry counterTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatCounter(counterTelemetry));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteExceptionAsync(ExceptionTelemetry exceptionTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatException(exceptionTelemetry));
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