using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IIntentTelemetryData : ICommonTelemetryData
    {
        string IntentName { get; set; }
        string IntentText { get; set; }
        double? IntentConfidenceScore { get; set; }
        bool IntentHasAmbiguousEntities { get; }
        IList<IEntityTelemetryData> IntentEntities { get; }
    }
}