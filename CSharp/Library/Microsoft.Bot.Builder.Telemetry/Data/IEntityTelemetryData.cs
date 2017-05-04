namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IEntityTelemetryData : ICommonTelemetry
    {
        string EntityType { get; set; }
        string EntityValue { get; set; }
        double EntityConfidenceScore { get; set; }
    }
}