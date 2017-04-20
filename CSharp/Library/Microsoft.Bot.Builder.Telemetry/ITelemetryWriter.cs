using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryWriter : ISetTelemetryContext
    {
        Task WriteIntentAsync(string intent, float score, Dictionary<string, string> entities = null);
        Task WriteEntityAsync(string kind, string value);
        Task WriteCounterAsync(string counter, int count = 1);
        Task WriteServiceResultAsync(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true);
        Task WriteExceptionAsync(string component, string context, Exception e);
        Task WriteEventAsync(string key, string value);
        Task WriteEventAsync(string key, double value);
        Task WriteEventAsync(Dictionary<string, double> metrics);
        Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}