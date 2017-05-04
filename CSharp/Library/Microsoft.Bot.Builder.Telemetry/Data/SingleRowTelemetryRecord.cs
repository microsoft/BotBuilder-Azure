using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public class SingleRowTelemetryRecord
        : ICommonTelemetry,
        IIntentTelemetry,
        IEntityTelemetry,
        IResponseTelemetry,
        ICounterTelemetry,
        IServiceResultTelemetry,
        IExceptionTelemetry,
        ITraceTelemetry
    {
        //ICommonTelemetry
        public string RecordType { get; set; }
        public DateTime Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string ChannelId { get; set; }
        public string ConversationId { get; set; }
        public string ActivityId { get; set; }
        public string UserId { get; set; }

        //IIntentTelemetry
        public string IntentName { get; set; }
        public string IntentText { get; set; }
        public double IntentScore { get; set; }
        public Dictionary<string, string> IntentEntities { get; set; }

        //IEntityTelemetry
        public string EntityType { get; set; }
        public string EntityValue { get; set; }

        //IResponseTelemetry
        public string ResponseText { get; set; }
        public string ResponseImageUrl { get; set; }
        public string ResponseJson { get; set; }
        public string ResponseResult { get; set; }
        public DateTime ResponseStartTime { get; set; }
        public DateTime ResponseEndDateTime { get; set; }
        public double ResponseMillisecondsDuration { get; set; }
        public bool ResponseIsCacheHit { get; set; }

        //ICounterTelemetry
        public string CounterName { get; set; }
        public int CounterValue { get; set; }

        //IServiceResultTelemetry
        public string ServiceResultName { get; set; }
        public bool ServiceResultSuccess { get; set; }
        public string ServiceResultResponse { get; set; }
        public DateTime ServiceResultStartDateTime { get; set; }
        public DateTime ServiceResultEndDateTime { get; set; }
        public double ServiceResultMillisecondsDuration { get; set; }

        //ITraceTelemetry
        public string TraceName { get; set; }
        public string TraceJson { get; set; }

        //IExceptionTelemetry
        public string ExceptionComponent { get; set; }
        public string ExceptionContext { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionDetail { get; set; }
        public Exception Ex { get; set; }


        public string AsStringWith(ITelemetryContext context)
        {
            var sb = new StringBuilder();

            sb.Append($"{RecordType}");

            sb.Append($"\t{context.Timestamp}");
            sb.Append($"\t{context.CorrelationId}");
            sb.Append($"\t{context.ChannelId}");
            sb.Append($"\t{context.ConversationId}");
            sb.Append($"\t{context.ActivityId}");
            sb.Append($"\t{context.UserId}");

            sb.Append($"\t{IntentName}");
            sb.Append($"\t{IntentText}");
            sb.Append($"\t{IntentScore}");

            sb.Append($"\t{EntityType}");
            sb.Append($"\t{EntityValue}");

            sb.Append($"\t{ResponseText}");
            sb.Append($"\t{ResponseImageUrl}");
            sb.Append($"\t{ResponseJson}");
            sb.Append($"\t{ResponseResult}");
            sb.Append($"\t{ResponseMillisecondsDuration}");
            sb.Append($"\t{ResponseIsCacheHit}");


            sb.Append($"\t{CounterName}");
            sb.Append($"\t{CounterValue}");

            sb.Append($"\t{ServiceResultName}");
            sb.Append($"\t{ServiceResultMillisecondsDuration}");
            sb.Append($"\t{ServiceResultSuccess}");
            sb.Append($"\t{ServiceResultResponse}");

            sb.Append($"\t{TraceName}");
            sb.Append($"\t{TraceJson}");

            sb.Append($"\t{ExceptionComponent}");
            sb.Append($"\t{ExceptionContext}");
            sb.Append($"\t{ExceptionType}");
            sb.Append($"\t{ExceptionMessage}");
            sb.Append($"\t{ExceptionDetail}");

            sb.Append($"{Environment.NewLine}");

            return sb.ToString();
        }







    }
}