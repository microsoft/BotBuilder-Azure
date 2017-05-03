using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryOutputFormatter : ISetTelemetryContext
    {
        string FormatServiceResult(string serviceName, DateTime startTime, DateTime endTime, string result, bool success = true);
        string FormatIntent(string intent, string text, double score);
        string FormatEntity(string kind, string value);
        string FormatCounter(string counter, int count);
        string FormatException(string component, string context, Exception ex);
        string FormatEvent(Dictionary<string, double> metrics);
        string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
        string FormatResponse(string text, string imageUrl, string json, string result, DateTime startTime, DateTime endDateTime, bool isCacheHit);
    }
}
