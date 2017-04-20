using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    [Flags]
    public enum TelemetryTypes
    {
        None = 0,
        Intents = 1,
        Entities = 2,
        Counters = 4,
        ServiceResults = 8,
        Exceptions = 16,
        CustomEvents = 32,
        All = ~0
    }
}