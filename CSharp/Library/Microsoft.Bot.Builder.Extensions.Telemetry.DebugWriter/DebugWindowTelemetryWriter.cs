using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.DebugWriter
{
    public class DebugWindowTelemetryWriter : StringOutputTelemetryWriterBase, ITelemetryWriter
    {
        private readonly DebugWindowTelemetryWriterConfiguration _configuration;

        public DebugWindowTelemetryWriter(DebugWindowTelemetryWriterConfiguration configuration, ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out OutputFormatter, nameof(formatter), formatter);
        }

        public Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            DoWriteTelemetry(counterTelemetryData, TelemetryTypes.Counters, OutputFormatter.FormatCounter);
            return Task.Delay(0);
        }

        public Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            DoWriteTelemetry(measureTelemetryData, TelemetryTypes.Measures, OutputFormatter.FormatMeasure);
            return Task.Delay(0);
        }

        public Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            DoWriteTelemetry(exceptionTelemetryData, TelemetryTypes.Exceptions, OutputFormatter.FormatException);
            return Task.Delay(0);
        }

        public Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            DoWriteTelemetry(serviceResultTelemetryData, TelemetryTypes.ServiceResults, OutputFormatter.FormatServiceResult);
            return Task.Delay(0);
        }

        public Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            DoWriteTelemetry(entityTelemetryData, TelemetryTypes.Entities, OutputFormatter.FormatEntity);
            return Task.Delay(0);
        }

        public Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            DoWriteTelemetry(intentTelemetryData, TelemetryTypes.Intents, OutputFormatter.FormatIntent);
            return Task.Delay(0);
        }

        public Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            DoWriteTelemetry(requestTelemetryData, TelemetryTypes.Requests, OutputFormatter.FormatRequest);
            return Task.Delay(0);
        }

        public Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            DoWriteTelemetry(responseTelemetryData, TelemetryTypes.Responses, OutputFormatter.FormatResponse);
            return Task.Delay(0);
        }

        public override Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                Debug.WriteLine(OutputFormatter.FormatEvent(properties, metrics));
            }

            return Task.Delay(0);
        }

        protected override Task DoWriteTelemetry<TTelemetryData>(TTelemetryData telemetryData, TelemetryTypes handleTypes, Func<TTelemetryData, string> formatter)
        {
            if (_configuration.Handles(handleTypes))
            {
                Debug.WriteLine(formatter(telemetryData));
            }

            return Task.Delay(0);
        }
    }
}
