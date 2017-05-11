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
        Measures = 32,
        ServiceResults = 64,
        Exceptions = 128,
        CustomEvents = 256,
        All = ~0,
        
    }
}