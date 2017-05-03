using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Telemetry.DebugWriter
{
    public class DebugWindowTelemetryWriter : ITelemetryWriter
    {
        private readonly DebugWindowTelemetryWriterConfiguration _configuration;
        private readonly ITelemetryOutputFormatter _outputFormatter;

        public DebugWindowTelemetryWriter(DebugWindowTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out _outputFormatter, nameof(formatter), formatter);
        }

        public async Task WriteCounterAsync(string counter, int count = 1)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                Debug.WriteLine(_outputFormatter.FormatCounter(counter, count));
            }
        }

        public async Task WriteExceptionAsync(string component, string context, Exception e)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                Debug.WriteLine(_outputFormatter.FormatException(component, context, e));
            }
        }

        public async Task WriteServiceResultAsync(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                Debug.WriteLine(_outputFormatter.FormatServiceResult(serviceName, startTime, endDateTime, result, success));

            }
        }

        public async Task WriteEntityAsync(string kind, string value)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                Debug.WriteLine(_outputFormatter.FormatEntity(kind, value));
            }
        }

        public async Task WriteIntentAsync(string intent, string text, double score, Dictionary<string, string> entities = null)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                Debug.WriteLine(_outputFormatter.FormatIntent(intent, text, score));

                if (null != entities)
                {
                    foreach (var entity in entities)
                    {
                        await WriteEntityAsync(entity.Key, entity.Value);
                    }
                }
            }
        }

        public async Task WriteResponseAsync(string text, string imageUrl, string json, string result, bool isCacheHit = false)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                Debug.WriteLine(_outputFormatter.FormatResponse(text, imageUrl, json, result, isCacheHit));
            }
        }

        public void SetContext(ITelemetryContext context)
        {
            _outputFormatter.SetContext(context);
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

        public async Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                Debug.WriteLine(_outputFormatter.FormatEvent(properties, metrics));
            }
        }
    }
}
