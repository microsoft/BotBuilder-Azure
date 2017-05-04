using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Data;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryWriter : ISetTelemetryContext
    {
        Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData);
        Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData);
        Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData);
        Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData);
        Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData);
        Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData);
        Task WriteEventAsync(string key, string value);
        Task WriteEventAsync(string key, double value);
        Task WriteEventAsync(Dictionary<string, double> metrics);
        Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}