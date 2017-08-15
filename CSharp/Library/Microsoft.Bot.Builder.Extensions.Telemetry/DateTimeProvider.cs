using System;

namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset Now()
        {
            return DateTimeOffset.Now;
        }
    }
}