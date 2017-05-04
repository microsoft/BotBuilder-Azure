using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IIntentTelemetryData : ICommonTelemetryData
    {
        string IntentName { get; set; }
        string IntentText { get; set; }
        double IntentScore { get; set; }
        IList<IEntityTelemetryData> IntentEntities { get; }
    }
}