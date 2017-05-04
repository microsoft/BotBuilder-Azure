using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IResponseTelemetryData : ICommonTelemetryData
    {
        string ResponseText { get; set; }
        string ResponseImageUrl { get; set; }
        string ResponseJson { get; set; }
        string ResponseResult { get; set; }
        string ResponseType { get; set; }
        DateTime ResponseStartDateTime { get; set; }
        DateTime ResponseEndDateTime { get; set; }
        bool ResponseIsCacheHit { get; set; }
        double ResponseMilliseconds { get; }
    }
}