namespace Microsoft.Bot.Builder.Telemetry
{
    interface ITraceTelemetry
    {
        string TraceName { get; set; }
        string TraceJson { get; set; }
    }
}