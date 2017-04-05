using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class TelemetryContext : ITelemetryContext
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public string ChannelId { get; set; }
        public string ConversationId { get; set; }
        public string ActivityId { get; set; }
        public string UserId { get; set; }
        public string Timestamp { get; }
        public string CorrelationId => CorrelationIdGenerator.GenerateCorrelationIdFrom(this);
        public ITelemetryContextCorrelationIdGenerator CorrelationIdGenerator { get; set; }


        public TelemetryContext(IDateTimeProvider dateTimeProvider, ITelemetryContextCorrelationIdGenerator correlationIdGenerator = null)
        {
            SetField.NotNull(out _dateTimeProvider, nameof(dateTimeProvider), dateTimeProvider);
            _dateTimeProvider = dateTimeProvider;

            //set the Timestamp only once so that it will remain consistent for the life of the object
            Timestamp = dateTimeProvider.GetCurrentTime().ToString("O");

            //set a default generator for the correlation id
            CorrelationIdGenerator = correlationIdGenerator ?? new AllIdsAndTimestampTelemetryContextCorrelationIdGenerator();
        }

        public ITelemetryContext CloneWithRefreshedTimestamp()
        {
            return new TelemetryContext(_dateTimeProvider)
            {
                ConversationId = this.ConversationId,
                ActivityId = this.ActivityId,
                ChannelId = this.ChannelId,
                UserId = this.UserId,
                CorrelationIdGenerator = this.CorrelationIdGenerator
            };
        }

    }
}