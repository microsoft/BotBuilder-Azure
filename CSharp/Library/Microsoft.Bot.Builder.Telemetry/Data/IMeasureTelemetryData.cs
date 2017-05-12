namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IMeasureTelemetryData : ICommonTelemetryData
    {
        string MeasureCategory { get; set; }
        string MeasureName { get; set; }
        double MeasureValue { get; set; }
    }
}