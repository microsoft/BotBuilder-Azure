using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Internals.Fibers;

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
        public string FormatCounter(ICounterTelemetry counterTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tCounter: [{counterTelemetry.CounterName}] - Count: [{counterTelemetry.CounterValue}]";
        }

        public string FormatEntity(IEntityTelemetry entityTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tEntity: [{entityTelemetry.EntityType}]-[{entityTelemetry.EntityValue}]";
        }

        public string FormatResponse(IResponseTelemetry responseTelemetry)
        {
            var duration = responseTelemetry.ResponseStartTime.Subtract(responseTelemetry.ResponseEndDateTime).TotalMilliseconds;
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tResponse: [{responseTelemetry.ResponseResult} / {responseTelemetry.ResponseImageUrl} / {responseTelemetry.ResponseJson} / {duration} / {responseTelemetry.ResponseIsCacheHit}] - [{responseTelemetry.ResponseText}] ";
        }

        public string FormatException(IExceptionTelemetry exceptionTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tException: [{exceptionTelemetry.ExceptionComponent} with [{exceptionTelemetry.ExceptionContext}]" + Environment.NewLine + $"\t{exceptionTelemetry.Ex}";
        }

        public string FormatIntent(IIntentTelemetry intentTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tIntent: [{intentTelemetry.IntentName} ({intentTelemetry.IntentScore})] - [{intentTelemetry.IntentText}]";
        }

        public string FormatServiceResult(IServiceResultTelemetry serviceResultTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tServiceResult: [{serviceResultTelemetry.ServiceResultName}] - result: [{serviceResultTelemetry.ServiceResultResponse}] - duration(ms): [{serviceResultTelemetry.ServiceResultEndDateTime.Subtract(serviceResultTelemetry.ServiceResultStartDateTime).TotalMilliseconds}] - success: [{serviceResultTelemetry.ServiceResultSuccess}]";
        }

        public void SetContext(ITelemetryContext context)
        {
            _context = context;
        }

        private string GetBotContextProperties()
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

            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tEvent: [{message}]";
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

            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tEvent: [{message}]";
        }
    }
}
