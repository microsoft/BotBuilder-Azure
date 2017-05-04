﻿using System;
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

        public async Task WriteCounterAsync(CounterTelemetry counterTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                Debug.WriteLine(_outputFormatter.FormatCounter(counterTelemetry.Counter, counterTelemetry.Count));
            }
        }

        public async Task WriteExceptionAsync(ExceptionTelemetry exceptionTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                Debug.WriteLine(_outputFormatter.FormatException(exceptionTelemetry.Component, exceptionTelemetry.Context, exceptionTelemetry.E));
            }
        }

        public async Task WriteServiceResultAsync(ResultTelemetry resultTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                Debug.WriteLine(_outputFormatter.FormatServiceResult(resultTelemetry.ServiceName, resultTelemetry.StartTime, resultTelemetry.EndDateTime, resultTelemetry.Result, resultTelemetry.Success));

            }
        }

        public async Task WriteEntityAsync(EntityTelemetry entityTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                Debug.WriteLine(_outputFormatter.FormatEntity(entityTelemetry.Kind, entityTelemetry.Value));
            }
        }

        public async Task WriteIntentAsync(IntentTelemetry intentTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                Debug.WriteLine(_outputFormatter.FormatIntent(intentTelemetry.Intent, intentTelemetry.Text, intentTelemetry.Score));

                if (null != intentTelemetry.Entities)
                {
                    foreach (var entity in intentTelemetry.Entities)
                    {
                        await WriteEntityAsync(new EntityTelemetry(entity.Key, entity.Value));
                    }
                }
            }
        }

        public async Task WriteResponseAsync(ResponseTelemetry responseTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                Debug.WriteLine(_outputFormatter.FormatResponse(responseTelemetry.Text, responseTelemetry.ImageUrl, responseTelemetry.Json, responseTelemetry.Result, responseTelemetry.StartTime, responseTelemetry.EndDateTime, responseTelemetry.IsCacheHit));
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
