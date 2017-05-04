namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IEntityTelemetry : ICommonTelemetry
    {
        string EntityType { get; set; }
        string EntityValue { get; set; }
    }
}