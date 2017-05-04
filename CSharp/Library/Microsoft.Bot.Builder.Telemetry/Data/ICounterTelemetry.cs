namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface ICounterTelemetry : ICommonTelemetry
    {
        string CounterName { get; set; }
        int CounterValue { get; set; }
    }
}