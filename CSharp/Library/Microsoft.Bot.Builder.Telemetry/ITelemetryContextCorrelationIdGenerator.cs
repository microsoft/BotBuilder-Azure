namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryContextCorrelationIdGenerator
    {
        string GenerateCorrelationIdFrom(ITelemetryContext context);
    }
}