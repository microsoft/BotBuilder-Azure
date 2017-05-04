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
        public string FormatCounter(CounterTelemetry counterTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tCounter: [{counterTelemetry.Counter}] - Count: [{counterTelemetry.Count}]";
        }

        public string FormatEntity(EntityTelemetry entityTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tEntity: [{entityTelemetry.Kind}]-[{entityTelemetry.Value}]";
        }

        public string FormatResponse(ResponseTelemetry responseTelemetry)
        {
            var duration = responseTelemetry.StartTime.Subtract(responseTelemetry.EndDateTime).TotalMilliseconds;
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tResponse: [{responseTelemetry.Result} / {responseTelemetry.ImageUrl} / {responseTelemetry.Json} / {duration} / {responseTelemetry.IsCacheHit}] - [{responseTelemetry.Text}] ";
        }

        public string FormatException(ExceptionTelemetry exceptionTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tException: [{exceptionTelemetry.Component} with [{exceptionTelemetry.Context}]" + Environment.NewLine + $"\t{exceptionTelemetry.Ex}";
        }

        public string FormatIntent(IntentTelemetry intentTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tIntent: [{intentTelemetry.Intent} ({intentTelemetry.Score})] - [{intentTelemetry.Text}]";
        }

        public string FormatServiceResult(ServiceResultTelemetry serviceResultTelemetry)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tServiceResult: [{serviceResultTelemetry.ServiceName}] - result: [{serviceResultTelemetry.Result}] - duration(ms): [{serviceResultTelemetry.EndTime.Subtract(serviceResultTelemetry.StartTime).TotalMilliseconds}] - success: [{serviceResultTelemetry.Success}]";
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
