using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class IntentTelemetry
    {
        public IntentTelemetry(string intent, string text, double score, Dictionary<string, string> entities = null)
        {
            Intent = intent;
            Text = text;
            Score = score;
            Entities = entities;
        }

        public string Intent { get; set; }
        public string Text { get; set; }
        public double Score { get; set; }
        public Dictionary<string, string> Entities { get; set; }
    }

    public class EntityTelemetry
    {
        public EntityTelemetry(string kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public string Kind { get; set; }
        public string Value { get; set; }
    }

    public class CounterTelemetry
    {
        public CounterTelemetry(string counter, int count = 1)
        {
            Counter = counter;
            Count = count;
        }

        public string Counter { get; set; }
        public int Count { get; set; }
    }

    public class ResponseTelemetry
    {
        public ResponseTelemetry(string text, string imageUrl, string json, string result, DateTime startTime, DateTime endDateTime, bool isCacheHit = false)
        {
            Text = text;
            ImageUrl = imageUrl;
            Json = json;
            Result = result;
            StartTime = startTime;
            EndDateTime = endDateTime;
            IsCacheHit = isCacheHit;
        }

        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string Json { get; set; }
        public string Result { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsCacheHit { get; set; }
    }

    public class ServiceResultTelemetry
    {
        public ServiceResultTelemetry(string serviceName, DateTime startDateTime, DateTime endDateTime, string result, bool success = true)
        {
            ServiceName = serviceName;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Result = result;
            Success = success;
        }

        public string ServiceName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Result { get; set; }
        public bool Success { get; set; }
    }

    public class ExceptionTelemetry
    {
        public ExceptionTelemetry(string component, string context, Exception ex)
        {
            Component = component;
            Context = context;
            Ex = ex;
        }

        public string Component { get; set; }
        public string Context { get; set; }
        public Exception Ex { get; set; }
    }

    public interface ITelemetryWriter : ISetTelemetryContext
    {
        Task WriteIntentAsync(IntentTelemetry intentTelemetry);
        Task WriteEntityAsync(EntityTelemetry entityTelemetry);
        Task WriteCounterAsync(CounterTelemetry counterTelemetry);
        Task WriteResponseAsync(ResponseTelemetry responseTelemetry);
        Task WriteServiceResultAsync(ServiceResultTelemetry serviceResultTelemetry);
        Task WriteExceptionAsync(ExceptionTelemetry exceptionTelemetry);
        Task WriteEventAsync(string key, string value);
        Task WriteEventAsync(string key, double value);
        Task WriteEventAsync(Dictionary<string, double> metrics);
        Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
    }
}