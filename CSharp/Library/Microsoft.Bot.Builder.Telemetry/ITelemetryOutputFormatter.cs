using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Telemetry.Data;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryOutputFormatter : ISetTelemetryContext
    {
        string FormatServiceResult(IServiceResultTelemetry serviceResultTelemetry);
        string FormatIntent(IIntentTelemetry intentTelemetry);
        string FormatEntity(IEntityTelemetry entityTelemetry);
        string FormatCounter(ICounterTelemetry counterTelemetry);
        string FormatException(IExceptionTelemetry exceptionTelemetry);
        string FormatEvent(Dictionary<string, double> metrics);
        string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
        string FormatResponse(IResponseTelemetry responseTelemetry);
    }
}
