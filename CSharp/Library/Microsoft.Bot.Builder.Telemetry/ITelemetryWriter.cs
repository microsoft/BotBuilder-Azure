using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Data;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryWriter : ISetTelemetryContext
    {
        Task WriteIntentAsync(IIntentTelemetry intentTelemetry);
        Task WriteEntityAsync(IEntityTelemetry entityTelemetry);
        Task WriteCounterAsync(ICounterTelemetry counterTelemetry);
        Task WriteResponseAsync(IResponseTelemetry responseTelemetry);
        Task WriteServiceResultAsync(IServiceResultTelemetry serviceResultTelemetry);
        Task WriteExceptionAsync(IExceptionTelemetry exceptionTelemetry);
        Task WriteEventAsync(string key, string value);
        Task WriteEventAsync(string key, double value);
        Task WriteEventAsync(Dictionary<string, double> metrics);
        Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}