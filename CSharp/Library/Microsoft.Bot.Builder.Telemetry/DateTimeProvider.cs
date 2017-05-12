using System;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset Now()
        {
            return DateTimeOffset.Now;
        }
    }
}