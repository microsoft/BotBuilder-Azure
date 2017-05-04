namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface ICounterTelemetryData : ICommonTelemetryData
    {
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }
}