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

        public string Intent { get; private set; }
        public string Text { get; private set; }
        public double Score { get; private set; }
        public Dictionary<string, string> Entities { get; private set; }
    }

    public class EntityTelemetry
    {
        public EntityTelemetry(string kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public string Kind { get; private set; }
        public string Value { get; private set; }
    }

    public class CounterTelemetry
    {
        public CounterTelemetry(string counter, int count = 1)
        {
            Counter = counter;
            Count = count;
        }

        public string Counter { get; private set; }
        public int Count { get; private set; }
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

        public string Text { get; private set; }
        public string ImageUrl { get; private set; }
        public string Json { get; private set; }
        public string Result { get; private set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        public bool IsCacheHit { get; private set; }
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

        public string ServiceName { get; private set; }
        public DateTime StartDateTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        public string Result { get; private set; }
        public bool Success { get; private set; }
    }

    public class ExceptionTelemetry
    {
        public ExceptionTelemetry(string component, string context, Exception ex)
        {
            Component = component;
            Context = context;
            Ex = ex;
        }

        public string Component { get; private set; }
        public string Context { get; private set; }
        public Exception Ex { get; private set; }
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