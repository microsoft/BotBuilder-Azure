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

        public async Task WriteServiceResultAsync(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatServiceResult(serviceName, startTime, endDateTime, result, success));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteIntentAsync(string intent, string text, double score, Dictionary<string, string> entities = null)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(async () =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatIntent(intent, text, score));

                    if (null != entities)
                    {
                        foreach (var entity in entities)
                        {
                            await WriteEntityAsync(entity.Key, entity.Value);
                        }
                    }


                    DoPostLogActions();
                });
            }
        }

        public async Task WriteEntityAsync(string kind, string value)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatEntity(kind, value));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteResponseAsync(string text, string imageUrl, string json, string result, bool isCacheHit = false)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatResponse(text, imageUrl, json, result, isCacheHit));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteCounterAsync(string counter, int count = 1)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatCounter(counter, count));
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteExceptionAsync(string component, string context, Exception e)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(() =>
                {
                    ThreadsafeWriteToFile(_outputFormatter.FormatException(component, context, e));
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