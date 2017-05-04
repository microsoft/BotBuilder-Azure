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

    public class IntentTelemetry : IIntentTelemetry
    {
        public IntentTelemetry(string intent, string text, double score, Dictionary<string, string> entities = null)
        {
            IntentName = intent;
            IntentText = text;
            IntentScore = score;
            IntentEntities = entities;
        }

        public string IntentName { get; set; }
        public string IntentText { get; set; }
        public double IntentScore { get; set; }
        public Dictionary<string, string> IntentEntities { get; set; }
    }

    public interface IEntityTelemetry
    {
        string EntityType { get; set; }
        string EntityValue { get; set; }
    }

    public class EntityTelemetry : IEntityTelemetry
    {
        public EntityTelemetry(string type, string value)
        {
            EntityType = type;
            EntityValue = value;
        }

        public string EntityType { get; set; }
        public string EntityValue { get; set; }
    }

    public interface ICounterTelemetry
    {
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }

    public class CounterTelemetry : ICounterTelemetry
    {
        public CounterTelemetry(string counter, int count = 1)
        {
            CounterName = counter;
            CounterValue = count;
        }

        public string CounterName { get; set; }
        public int CounterValue { get; set; }
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

    public class ResponseTelemetry : IResponseTelemetry
    {
        public ResponseTelemetry(string text, string imageUrl, string json, string result, DateTime startTime, DateTime endDateTime, bool isCacheHit = false)
        {
            ResponseText = text;
            ResponseImageUrl = imageUrl;
            ResponseJson = json;
            ResponseResult = result;
            ResponseStartTime = startTime;
            ResponseEndDateTime = endDateTime;
            ResponseIsCacheHit = isCacheHit;
        }

        public string ResponseText { get; set; }
        public string ResponseImageUrl { get; set; }
        public string ResponseJson { get; set; }
        public string ResponseResult { get; set; }
        public DateTime ResponseStartTime { get; set; }
        public DateTime ResponseEndDateTime { get; set; }
        public double ResponseMillisecondsDuration { get; set; }
        public bool ResponseIsCacheHit { get; set; }
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

    public class ServiceResultTelemetry : IServiceResultTelemetry
    {
        public ServiceResultTelemetry(string serviceName, DateTime startDateTime, DateTime endDateTime, string result, bool success = true)
        {
            ServiceResultName = serviceName;
            ServiceResultStartDateTime = startDateTime;
            ServiceResultEndDateTime = endDateTime;
            ServiceResultResponse = result;
            ServiceResultSuccess = success;
        }

        public string ServiceResultName { get; set; }
        public DateTime ServiceResultStartDateTime { get; set; }
        public DateTime ServiceResultEndDateTime { get; set; }
        public double ServiceResultMillisecondsDuration { get; set; }
        public string ServiceResultResponse { get; set; }
        public bool ServiceResultSuccess { get; set; }
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

    public class ExceptionTelemetry : IExceptionTelemetry
    {
        public ExceptionTelemetry(string exceptionComponent, string exceptionContext, Exception ex)
        {
            ExceptionComponent = exceptionComponent;
            ExceptionContext = exceptionContext;
            Ex = ex;
        }

        public string ExceptionComponent { get; set; }
        public string ExceptionContext { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionDetail { get; set; }
        public Exception Ex { get; set; }
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