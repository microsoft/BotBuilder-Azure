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
        public string FormatCounter(string counter, int count)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tCounter: [{counter}] - Count: [{count}]";
        }

        public string FormatEntity(string kind, string value)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tEntity: [{kind}]-[{value}]";
        }

        public string FormatException(string component, string context, Exception ex)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tException: [{component} with [{context}]" + Environment.NewLine + $"\t{ex}";
        }

        public string FormatIntent(string intent, double score)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tIntent: [{intent}] - [{score}]";
        }

        public string FormatServiceResult(string serviceName, DateTime startTime, DateTime endTime, string result, bool success = true)
        {
            return $"{GetDateTimeString()}\t{GetBotContextProperties()}\tServiceResult: [{serviceName}] - result: [{result}] - duration(ms): [{endTime.Subtract(startTime).TotalMilliseconds}] - success: [{success}]";
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
