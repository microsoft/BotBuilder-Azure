using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IServiceResultTelemetryData : ICommonTelemetry
    {
        string ServiceResultName { get; set; }
        double ServiceResultMilliseconds { get; }
        bool ServiceResultSuccess { get; set; }
        string ServiceResultResponse { get; set; }
        DateTime ServiceResultStartDateTime { get; set; }
        DateTime ServiceResultEndDateTime { get; set; }
    }
}