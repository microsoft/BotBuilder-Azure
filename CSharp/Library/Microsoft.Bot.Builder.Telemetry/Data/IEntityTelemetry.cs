namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IEntityTelemetry
    {
        string EntityType { get; set; }
        string EntityValue { get; set; }
    }
}