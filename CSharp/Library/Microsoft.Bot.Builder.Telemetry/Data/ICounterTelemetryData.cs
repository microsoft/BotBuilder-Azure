namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface ICounterTelemetryData : ICommonTelemetry
    {
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }
}