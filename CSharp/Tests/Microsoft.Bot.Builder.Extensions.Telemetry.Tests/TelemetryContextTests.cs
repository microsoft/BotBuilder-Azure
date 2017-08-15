using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Tests
{
    [TestClass]
    public class TelemetryContextTests
    {
        private TelemetryContext _telemetryContext;

        [TestInitialize]
        public void SetUp()
        {
            _telemetryContext = new TelemetryContext(new DateTimeProvider())
            {
                ChannelId = "I am the Channel Id",
                ActivityId = "I am the Activity Id",
                ConversationId = "I am the Conversation Id",
                UserId = "I am the User Id",
            };
        }

        [TestMethod]
        public void CanRetrieveTimestamp()
        {
            Assert.IsNotNull(_telemetryContext.Timestamp);
        }

        [TestMethod]
        public void MultipleRequestForTimestampReturnSameValue()
        {
            var firstRequest = _telemetryContext.Timestamp;

            Thread.Sleep(1000); //need to pause sufficiently here to ensure the clock moves forward

            var secondRequest = _telemetryContext.Timestamp;

            Thread.Sleep(1000);

            var thirdRequest = _telemetryContext.Timestamp;

            Assert.AreEqual(firstRequest, secondRequest);
            Assert.AreEqual(secondRequest, thirdRequest);
        }

        [TestMethod]
        public void CanCloneWithOnlyDifferentTimestampAndCorrelationIdResult()
        {
            Thread.Sleep(1000); //need to pause sufficiently here to ensure the clock moves forward

            var clonedContext = _telemetryContext.CloneWithRefreshedTimestamp();

            Assert.AreEqual(clonedContext.ActivityId, _telemetryContext.ActivityId);
            Assert.AreEqual(clonedContext.ConversationId, _telemetryContext.ConversationId);
            Assert.AreEqual(clonedContext.ChannelId, _telemetryContext.ChannelId);
            Assert.AreEqual(clonedContext.UserId, _telemetryContext.UserId);
            Assert.AreEqual(clonedContext.CorrelationIdGenerator, _telemetryContext.CorrelationIdGenerator);

            Assert.AreNotEqual(clonedContext.Timestamp, _telemetryContext.Timestamp);
            Assert.AreNotEqual(clonedContext.CorrelationId, _telemetryContext.CorrelationId);
        }

        [TestMethod]
        public void CanUseDefaultCorrelationIdGenerator()
        {
            Assert.IsNotNull(_telemetryContext.CorrelationId);

            var expectedCorrelationId = $"{_telemetryContext.ChannelId}{_telemetryContext.ConversationId}{_telemetryContext.ActivityId}{_telemetryContext.UserId}{_telemetryContext.Timestamp}";
            Assert.AreEqual(_telemetryContext.CorrelationId, expectedCorrelationId);
        }

        [TestMethod]
        public void CanUseCustomCorrelationIdGenerator()
        {
            var customCorrelationIdGenerator = new TestCorrelationIdGenerator();
            var customCorrelationId = customCorrelationIdGenerator.GenerateCorrelationIdFrom(_telemetryContext);

            Assert.AreNotEqual(_telemetryContext.CorrelationId, customCorrelationId);

            _telemetryContext.CorrelationIdGenerator = customCorrelationIdGenerator;

            Assert.AreEqual(_telemetryContext.CorrelationId, customCorrelationId);
        }

        private class TestCorrelationIdGenerator : ITelemetryContextCorrelationIdGenerator
        {
            public string GenerateCorrelationIdFrom(ITelemetryContext context)
            {
                return "I am the custom correlation id";
            }
        }
    }
}