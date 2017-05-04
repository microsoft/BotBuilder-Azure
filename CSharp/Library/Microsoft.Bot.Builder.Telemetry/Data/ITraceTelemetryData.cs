namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface ITraceTelemetryData
    {
        string TraceName { get; set; }
        string TraceJson { get; set; }
    }
}