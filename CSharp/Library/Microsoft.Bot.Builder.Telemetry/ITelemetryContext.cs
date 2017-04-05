namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryContext
    {
        string ChannelId { get; set; }
        string ConversationId { get; set; }
        string ActivityId { get; set; }
        string Timestamp { get; }
        string CorrelationId { get; }
        string UserId { get; set; }
        ITelemetryContextCorrelationIdGenerator CorrelationIdGenerator { get; set; }
        ITelemetryContext CloneWithRefreshedTimestamp();
    }
}