namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IEntityTelemetryData : ICommonTelemetryData
    {
        string EntityType { get; set; }
        string EntityValue { get; set; }
        double? EntityConfidenceScore { get; set; }
        bool EntityIsAmbiguous { get; set; }
    }
}