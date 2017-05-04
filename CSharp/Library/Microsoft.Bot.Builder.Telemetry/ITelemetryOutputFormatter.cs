using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryOutputFormatter : ISetTelemetryContext
    {
        string FormatServiceResult(ServiceResultTelemetry serviceResultTelemetry);
        string FormatIntent(IntentTelemetry intentTelemetry);
        string FormatEntity(EntityTelemetry entityTelemetry);
        string FormatCounter(CounterTelemetry counterTelemetry);
        string FormatException(ExceptionTelemetry exceptionTelemetry);
        string FormatEvent(Dictionary<string, double> metrics);
        string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
        string FormatResponse(ResponseTelemetry responseTelemetry);
    }
}
