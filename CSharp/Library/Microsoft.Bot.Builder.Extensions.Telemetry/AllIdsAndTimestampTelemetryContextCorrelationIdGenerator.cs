namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public class AllIdsAndTimestampTelemetryContextCorrelationIdGenerator : ITelemetryContextCorrelationIdGenerator
    {
        public string GenerateCorrelationIdFrom(ITelemetryContext context)
        {
            return $"{context.ChannelId}{context.ConversationId}{context.ActivityId}{context.UserId}{context.Timestamp}";
        }
    }
}