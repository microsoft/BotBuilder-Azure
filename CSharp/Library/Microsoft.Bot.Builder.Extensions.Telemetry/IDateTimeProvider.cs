using System;

namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public interface IDateTimeProvider
    {
        DateTimeOffset Now();
    }
}