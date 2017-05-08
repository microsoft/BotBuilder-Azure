using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public class TelemetryData :
        IRequestTelemetryData,
        IIntentTelemetryData,
        IEntityTelemetryData,
        IResponseTelemetryData,
        ICounterTelemetryData,
        IMeasureTelemetryData,
        IServiceResultTelemetryData,
        IExceptionTelemetryData,
        ITraceTelemetryData
    {

        public TelemetryData()
        {
            IntentEntities = new List<IEntityTelemetryData>();
        }

        //ICommonTelemetryData
        public string RecordType { get; set; }
        public DateTime Timestamp { get; set; }
        public string CorrelationId { get; set; }
        public string ChannelId { get; set; }
        public string ConversationId { get; set; }
        public string ActivityId { get; set; }
        public string UserId { get; set; }

        //IRequestTelemetryData
        public DateTime RequestStartDateTime { get; set; }
        public DateTime RequestEndDateTime { get; set; }
        public bool RequestIsCacheHit { get; set; }
        public double RequestMilliseconds => RequestStartDateTime.Subtract(RequestEndDateTime).TotalMilliseconds;

        //IIntentTelemetryData
        public string IntentName { get; set; }
        public string IntentText { get; set; }
        public double? IntentConfidenceScore { get; set; }
        public bool IntentIsAmbiguous { get; set; }
        public bool IntentHasAmbiguousEntities => IntentEntities.Any(entity => entity.EntityIsAmbiguous);
        public IList<IEntityTelemetryData> IntentEntities { get; }

        //IEntityTelemetryData
        public string EntityType { get; set; }
        public string EntityValue { get; set; }
        public double? EntityConfidenceScore { get; set; }
        public bool EntityIsAmbiguous { get; set; }

        //IResponseTelemetryData
        public string ResponseText { get; set; }
        public string ResponseImageUrl { get; set; }
        public string ResponseJson { get; set; }
        public string ResponseResult { get; set; }
        public string ResponseType { get; set; }
 
        //ICounterTelemetryData
        public string CounterCategory { get; set; }
        public string CounterName { get; set; }
        public int CounterValue { get; set; }

        //IMeasureTelemetryData
        public string MeasureCategory { get; set; }
        public string MeasureName { get; set; }
        public int MeasureValue { get; set; }

        //IServiceResultTelemetryData
        public string ServiceResultName { get; set; }
        public double ServiceResultMilliseconds => ServiceResultStartDateTime.Subtract(ServiceResultEndDateTime).TotalMilliseconds;
        public bool ServiceResultSuccess { get; set; }
        public string ServiceResultResponse { get; set; }
        public DateTime ServiceResultStartDateTime { get; set; }
        public DateTime ServiceResultEndDateTime { get; set; }

        //ITraceTelemetryData
        public string TraceName { get; set; }
        public string TraceJson { get; set; }

        //IExceptionTelemetryData
        public string ExceptionComponent { get; set; }
        public string ExceptionContext { get; set; }
        public Type ExceptionType => Ex?.GetType();
        public string ExceptionMessage => Ex?.Message;
        public string ExceptionDetail => Ex?.ToString();
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

            sb.Append($"\t{RequestIsCacheHit}");
            sb.Append($"\t{RequestMilliseconds}");

            sb.Append($"\t{IntentName}");
            sb.Append($"\t{IntentText}");
            sb.Append($"\t{IntentConfidenceScore}");
            sb.Append($"\t{IntentIsAmbiguous}");

            sb.Append($"\t{EntityType}");
            sb.Append($"\t{EntityValue}");
            sb.Append($"\t{EntityConfidenceScore}");
            sb.Append($"\t{EntityIsAmbiguous}");

            sb.Append($"\t{ResponseText}");
            sb.Append($"\t{ResponseImageUrl}");
            sb.Append($"\t{ResponseJson}");
            sb.Append($"\t{ResponseResult}");
            sb.Append($"\t{ResponseType}");
     
            sb.Append($"\t{CounterCategory}");
            sb.Append($"\t{CounterName}");
            sb.Append($"\t{CounterValue}");

            sb.Append($"\t{MeasureCategory}");
            sb.Append($"\t{MeasureName}");
            sb.Append($"\t{MeasureValue}");

            sb.Append($"\t{ServiceResultName}");
            sb.Append($"\t{ServiceResultMilliseconds}");
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

        public static IIntentTelemetryData NewIntentData() { return new TelemetryData(); }
        public static IEntityTelemetryData NewEntityData() { return new TelemetryData(); }
        public static IResponseTelemetryData NewResponseData() { return new TelemetryData(); }
        public static ICounterTelemetryData NewCounterData() { return new TelemetryData(); }
        public static IMeasureTelemetryData NewMeasureData() { return new TelemetryData(); }
        public static IServiceResultTelemetryData NewServiceResultData() { return new TelemetryData(); }
        public static IExceptionTelemetryData NewExceptionData() { return new TelemetryData(); }
        public static ITraceTelemetryData NewTraceData() { return new TelemetryData(); }

    }
}