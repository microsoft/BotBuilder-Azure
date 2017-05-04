using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IResponseTelemetry : ICommonTelemetry
    {
        string ResponseText { get; set; }
        string ResponseImageUrl { get; set; }
        string ResponseJson { get; set; }
        string ResponseResult { get; set; }
        DateTime ResponseStartTime { get; set; }
        DateTime ResponseEndDateTime { get; set; }
        double ResponseMillisecondsDuration { get; set; }
        bool ResponseIsCacheHit { get; set; }
    }
}