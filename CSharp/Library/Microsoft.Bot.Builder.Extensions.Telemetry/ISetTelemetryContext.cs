namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public interface ISetTelemetryContext
    {
        void SetContext(ITelemetryContext context);
    }
}