namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public interface ITelemetryContextCorrelationIdGenerator
    {
        string GenerateCorrelationIdFrom(ITelemetryContext context);
    }
}