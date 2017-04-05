using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Telemetry.Writers
{
    public class DebugWindowTelemetryWriter : ITelemetryWriter
    {
        private bool _disposed;
        private readonly ITelemetryOutputFormatter _outputFormatter;

        public DebugWindowTelemetryWriter(ITelemetryOutputFormatter formatter)
        {
            SetField.NotNull(out _outputFormatter, nameof(formatter), formatter);
        }

        public async Task WriteCounterAsync(string counter)
        {
            Debug.WriteLine(_outputFormatter.FormatCounter(counter));
        }

        public async Task WriteExceptionAsync(string component, string context, Exception e)
        {
            Debug.WriteLine(_outputFormatter.FormatException(component, context, e));
        }

        public async Task WriteServiceResultAsync(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true)
        {
            Debug.WriteLine(_outputFormatter.FormatServiceResult(serviceName, startTime, endDateTime, result, success));
        }

        public async Task WriteEntityAsync(string kind, string value)
        {
            Debug.WriteLine(_outputFormatter.FormatEntity(kind, value));
        }

        public async Task WriteIntentAsync(string intent, float score, Dictionary<string, string> entities = null)
        {
            Debug.WriteLine(_outputFormatter.FormatLogIntent(intent, score));

            if (null != entities)
            {
                foreach (var entity in entities)
                {
                    await WriteEntityAsync(entity.Key, entity.Value);
                }
            }
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
        }

        ~DebugWindowTelemetryWriter()
        {
            Dispose(false);
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
            Debug.WriteLine(_outputFormatter.FormatEvent(properties, metrics));
        }
    }
}
