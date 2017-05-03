using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    [Flags]
    public enum TelemetryTypes
    {
        None = 0,
        Intents = 1,
        Entities = 2,
        Responses = 4,
        Counters = 8,
        ServiceResults = 16,
        Exceptions = 32,
        CustomEvents = 64,
        All = ~0
    }
}