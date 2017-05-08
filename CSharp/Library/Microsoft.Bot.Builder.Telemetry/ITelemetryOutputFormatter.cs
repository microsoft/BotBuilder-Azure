using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Telemetry.Data;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryOutputFormatter : ISetTelemetryContext
    {
        string FormatServiceResult(IServiceResultTelemetryData serviceResultTelemetryData);
        string FormatRequest(IRequestTelemetryData requestTelemetryData);
        string FormatIntent(IIntentTelemetryData intentTelemetryData);
        string FormatEntity(IEntityTelemetryData entityTelemetryData);
        string FormatCounter(ICounterTelemetryData counterTelemetryData);
        string FormatMeasure(IMeasureTelemetryData measureTelemetryData);
        string FormatException(IExceptionTelemetryData exceptionTelemetryData);
        string FormatEvent(Dictionary<string, double> metrics);
        string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
        string FormatResponse(IResponseTelemetryData responseTelemetryData);
    }
}
