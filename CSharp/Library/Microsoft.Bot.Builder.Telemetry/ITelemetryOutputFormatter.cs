using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryOutputFormatter : ISetTelemetryContext
    {
        string FormatServiceResult(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true);
        string FormatLogIntent(string intent, float score);
        string FormatEntity(string kind, string value);
        string FormatCounter(string counter);
        string FormatException(string component, string context, Exception e);
        string FormatEvent(Dictionary<string, double> metrics);
        string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}
