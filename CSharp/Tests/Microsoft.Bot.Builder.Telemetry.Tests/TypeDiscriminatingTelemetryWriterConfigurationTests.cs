using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests
{
    [TestClass]
    public class TypeDiscriminatingTelemetryWriterConfigurationTests
    {
        private TestConfiguration _testConfiguration;

        [TestInitialize]
        public void SetUp()
        {
            _testConfiguration = new TestConfiguration();
        }

        [TestMethod]
        public void HandlesAllTypesByDefault()
        {
            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.All));
        }

        [TestMethod]
        public void CanHandleSingleExplicitType()
        {
            _testConfiguration.TelemetryTypesToHandle = TelemetryTypes.Counters;
            Assert.IsFalse(_testConfiguration.Handles(TelemetryTypes.All));
            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.Counters));
        }

        [TestMethod]
        public void CanHandleMultipleExplicitTypes()
        {
            _testConfiguration.TelemetryTypesToHandle = TelemetryTypes.Counters | TelemetryTypes.Entities;
            Assert.IsFalse(_testConfiguration.Handles(TelemetryTypes.All));
            Assert.IsFalse(_testConfiguration.Handles(TelemetryTypes.Intents));
            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.Counters));
            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.Entities));
        }


        [TestMethod]
        public void AllTypesIncludesIndividualTypes()
        {
            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.All));

            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.Counters));
            Assert.IsTrue(_testConfiguration.Handles(TelemetryTypes.Entities));
        }

        public class TestConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
        {

        }
    }
}