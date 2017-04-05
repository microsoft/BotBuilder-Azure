using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset GetCurrentTime()
        {
            return DateTimeOffset.Now;
        }
    }
}