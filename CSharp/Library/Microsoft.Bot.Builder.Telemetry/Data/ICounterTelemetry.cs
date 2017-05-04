namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface ICounterTelemetry
    {
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }
}