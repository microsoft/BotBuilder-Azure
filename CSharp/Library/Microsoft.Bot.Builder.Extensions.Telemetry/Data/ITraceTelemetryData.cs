namespace Microsoft.Bot.Builder.Extensions.Telemetry.Data
{
    public interface ITraceTelemetryData
    {
        string TraceName { get; set; }
        string TraceJson { get; set; }
    }
}