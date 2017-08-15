namespace Microsoft.Bot.Builder.Extensions.Telemetry.Data
{
    public interface ICounterTelemetryData : ICommonTelemetryData
    {
        string CounterCategory { get; set; }
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }
}