using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryReporter : ISetTelemetryContext
    {
        Task AddLuisEventDetailsAsync(string intent, float confidence, Dictionary<string, string> entities);
        Task AddDialogImpressionAsync(string dialog);
        Task AddServiceResultAsync(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true);
        Task AddEventAsync(string key, string value);
        Task AddEventAsync(string key, double value);
        Task AddEventAsync(Dictionary<string, double> metrics);
        Task AddEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}
