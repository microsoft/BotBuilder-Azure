using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IIntentTelemetry : ICommonTelemetry
    {
        string IntentName { get; set; }
        string IntentText { get; set; }
        double IntentScore { get; set; }
        Dictionary<string, string> IntentEntities { get; set; }
    }
}