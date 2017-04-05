using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface IDateTimeProvider
    {
        DateTimeOffset GetCurrentTime();
    }
}