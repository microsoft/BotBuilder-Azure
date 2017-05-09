using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IRequestTelemetryData : ICommonTelemetryData
    {
        DateTime RequestStartDateTime { get; set; }
        DateTime RequestEndDateTime { get; set; }
        bool RequestIsCacheHit { get; set; }
        string RequestQuality { get; set; }
        bool RequestIsAmbiguous { get; set; }
        bool RequestIsAuthenticated { get; set; }
        double RequestMilliseconds { get; }
    }
}