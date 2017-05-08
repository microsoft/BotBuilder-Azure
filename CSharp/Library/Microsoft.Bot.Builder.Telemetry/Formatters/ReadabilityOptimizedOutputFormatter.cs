using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry.Data;

namespace Microsoft.Bot.Builder.Telemetry.Formatters
{
    public class ReadabilityOptimizedOutputFormatter : ITelemetryOutputFormatter
    {
        private ITelemetryContext _context;

        public ReadabilityOptimizedOutputFormatter(ITelemetryContext context)
        {
            SetField.NotNull(out _context, nameof(context), context);
        }

        private string GetDateTimeString()
        {
            return DateTimeOffset.Now.ToString("O");
        }
        public string FormatCounter(ICounterTelemetryData counterTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tCounter: [{counterTelemetryData.CounterCategory}/{counterTelemetryData.CounterName}] - Count: [{counterTelemetryData.CounterValue}]";
        }

        public string FormatMeasure(IMeasureTelemetryData measureTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tMeasure: [{measureTelemetryData.MeasureCategory}/{measureTelemetryData.MeasureName}] - Value: [{measureTelemetryData.MeasureValue}]";
        }

        public string FormatEntity(IEntityTelemetryData entityTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tEntity: [{entityTelemetryData.EntityType} ({entityTelemetryData.EntityConfidenceScore}) / Ambiguous: {entityTelemetryData.EntityIsAmbiguous}]-[{entityTelemetryData.EntityValue}]";
        }

        public string FormatRequest(IRequestTelemetryData requestTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tRequest: [ Duration (ms): {requestTelemetryData.RequestMilliseconds} / cache hit?: {requestTelemetryData.RequestIsCacheHit}]";
        }

        public string FormatResponse(IResponseTelemetryData responseTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tResponse: [ result: {responseTelemetryData.ResponseResult} / image URL: {responseTelemetryData.ResponseImageUrl} / JSON: {responseTelemetryData.ResponseJson} / result: {responseTelemetryData.ResponseResult} / type: {responseTelemetryData.ResponseType}] - [{responseTelemetryData.ResponseText}] ";
        }

        public string FormatException(IExceptionTelemetryData exceptionTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tException: [{exceptionTelemetryData.ExceptionComponent} with [{exceptionTelemetryData.ExceptionContext}]" + Environment.NewLine + $"\t{exceptionTelemetryData.Ex}";
        }

        public string FormatIntent(IIntentTelemetryData intentTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tIntent: [{intentTelemetryData.IntentName} ({intentTelemetryData.IntentConfidenceScore}) / Ambiguous: {intentTelemetryData.IntentIsAmbiguous} / Ambiguous Entities: {intentTelemetryData.IntentHasAmbiguousEntities}] - [{intentTelemetryData.IntentText}]";
        }

        public string FormatServiceResult(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tServiceResult: [{serviceResultTelemetryData.ServiceResultName}] - result: [{serviceResultTelemetryData.ServiceResultResponse}] - duration(ms): [{serviceResultTelemetryData.ServiceResultEndDateTime.Subtract(serviceResultTelemetryData.ServiceResultStartDateTime).TotalMilliseconds}] - success: [{serviceResultTelemetryData.ServiceResultSuccess}]";
        }

        public void SetContext(ITelemetryContext context)
        {
            _context = context;
        }

        private string GetTelemetryContextProperties()
        {
            return $"correlationId: {_context.CorrelationId}\tchannelId: {_context.ChannelId}\tconversationId: {_context.ConversationId}\tactivityId: {_context.ActivityId}\tuserId: {_context.UserId}\ttimestamp: {_context.Timestamp}";
        }

        public string FormatEvent(Dictionary<string, double> metrics)
        {
            var message = new StringBuilder();

            foreach (var metric in metrics)
            {
                message.Append($"[{metric.Key},{metric.Value}]");
            }

            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tEvent: [{message}]";
        }

        public string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            var message = new StringBuilder();

            foreach (var property in properties)
            {
                message.Append($"[{property.Key},{property.Value}]");
            }

            if (null != metrics)
            {
                foreach (var metric in metrics)
                {
                    message.Append($"[{metric.Key},{metric.Value}]");
                }
            }

            return $"{GetDateTimeString()}\t{GetTelemetryContextProperties()}\tEvent: [{message}]";
        }
    }
}
