using System;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public class TelemetryException: Exception
    {
        public TelemetryException()
        {
        }

        public TelemetryException(string message) : base(message)
        {
        }

        public TelemetryException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TelemetryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
