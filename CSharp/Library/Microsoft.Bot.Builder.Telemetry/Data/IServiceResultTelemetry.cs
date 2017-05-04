using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IServiceResultTelemetry : ICommonTelemetry
    {
        string ServiceResultName { get; set; }
        DateTime ServiceResultStartDateTime { get; set; }
        DateTime ServiceResultEndDateTime { get; set; }
        double ServiceResultMillisecondsDuration { get; set; }
        string ServiceResultResponse { get; set; }
        bool ServiceResultSuccess { get; set; }
    }
}