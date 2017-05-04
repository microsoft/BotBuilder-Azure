using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry
{

    public interface ICommonTelemetry
    {
        string RecordType { get; set; }
        DateTime Timestamp { get; set; }
        string CorrelationId { get; set; }
        string ChannelId { get; set; }
        string ConversationId { get; set; }
        string ActivityId { get; set; }
        string UserId { get; set; }
    }

    public interface IIntentTelemetry
    {
        string IntentName { get; set; }
        string IntentText { get; set; }
        double IntentScore { get; set; }
        Dictionary<string, string> IntentEntities { get; set; }
    }

    
    public interface IEntityTelemetry
    {
        string EntityType { get; set; }
        string EntityValue { get; set; }
    }

    public interface ICounterTelemetry
    {
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }

    public interface IResponseTelemetry
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

    public interface IServiceResultTelemetry
    {
        string ServiceResultName { get; set; }
        DateTime ServiceResultStartDateTime { get; set; }
        DateTime ServiceResultEndDateTime { get; set; }
        double ServiceResultMillisecondsDuration { get; set; }
        string ServiceResultResponse { get; set; }
        bool ServiceResultSuccess { get; set; }
    }

   public interface IExceptionTelemetry
    {
        string ExceptionComponent { get; set; }
        string ExceptionContext { get; set; }
        Exception Ex { get; set; }
    }

    interface ITraceTelemetry
    {
        string TraceName { get; set; }
        string TraceJson { get; set; }
    }

    public interface ITelemetryWriter : ISetTelemetryContext
    {
        Task WriteIntentAsync(IIntentTelemetry intentTelemetry);
        Task WriteEntityAsync(IEntityTelemetry entityTelemetry);
        Task WriteCounterAsync(ICounterTelemetry counterTelemetry);
        Task WriteResponseAsync(IResponseTelemetry responseTelemetry);
        Task WriteServiceResultAsync(IServiceResultTelemetry serviceResultTelemetry);
        Task WriteExceptionAsync(IExceptionTelemetry exceptionTelemetry);
        Task WriteEventAsync(string key, string value);
        Task WriteEventAsync(string key, double value);
        Task WriteEventAsync(Dictionary<string, double> metrics);
        Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}