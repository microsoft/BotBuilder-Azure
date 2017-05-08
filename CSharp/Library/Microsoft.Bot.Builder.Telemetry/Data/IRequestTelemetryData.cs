using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IRequestTelemetryData : ICommonTelemetryData
    {
        DateTime RequestStartDateTime { get; set; }
        DateTime RequestEndDateTime { get; set; }
        bool RequestIsCacheHit { get; set; }
        double RequestMilliseconds { get; }
    }
}