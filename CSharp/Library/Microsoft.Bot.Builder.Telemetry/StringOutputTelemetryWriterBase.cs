using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry
{
    public abstract class StringOutputTelemetryWriterBase
    {
        protected ITelemetryOutputFormatter OutputFormatter;
        protected abstract Task DoWriteTelemetry<TTelemetryData>(TTelemetryData telemetryData, TelemetryTypes handleTypes, Func<TTelemetryData, string> formatter);

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

        public abstract Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);

        public virtual void SetContext(ITelemetryContext context)
        {
            OutputFormatter.SetContext(context);
        }
    }
}