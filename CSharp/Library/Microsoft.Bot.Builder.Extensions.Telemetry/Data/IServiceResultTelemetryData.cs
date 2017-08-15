using System;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Data
{
    public interface IServiceResultTelemetryData : ICommonTelemetryData
    {
        string ServiceResultName { get; set; }
        double ServiceResultMilliseconds { get; }
        bool ServiceResultSuccess { get; set; }
        string ServiceResultResponse { get; set; }
        DateTime ServiceResultStartDateTime { get; set; }
        DateTime ServiceResultEndDateTime { get; set; }
    }
}