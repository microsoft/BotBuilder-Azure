using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry.Data;

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

        public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                Debug.WriteLine(_outputFormatter.FormatCounter(counterTelemetryData));
            }
        }

        public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                Debug.WriteLine(_outputFormatter.FormatMeasure(measureTelemetryData));
            }
        }

        public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                Debug.WriteLine(_outputFormatter.FormatException(exceptionTelemetryData));
            }
        }

        public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                Debug.WriteLine(_outputFormatter.FormatServiceResult(serviceResultTelemetryData));

            }
        }

        public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                Debug.WriteLine(_outputFormatter.FormatEntity(entityTelemetryData));
            }
        }

        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                Debug.WriteLine(_outputFormatter.FormatIntent(intentTelemetryData));

                if (null != intentTelemetryData.IntentEntities)
                {
                    foreach (var entity in intentTelemetryData.IntentEntities)
                    {
                        await WriteEntityAsync(entity);
                    }
                }
            }
        }

        public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                Debug.WriteLine(_outputFormatter.FormatResponse(responseTelemetryData));
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
