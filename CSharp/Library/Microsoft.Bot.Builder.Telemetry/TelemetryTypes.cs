using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    [Flags]
    public enum TelemetryTypes
    {
        None = 0,
        Requests=1,
        Intents = 2,
        Entities = 4,
        Responses = 8,
        Counters = 16,
        ServiceResults = 32,
        Exceptions = 64,
        CustomEvents = 128,
        All = ~0,
    }
}