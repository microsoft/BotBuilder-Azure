using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests.TelemetryModuleTests
{
    [TestClass]
    public class DiscoveryStrategyTests
    {
        [TestMethod]
        public void CanSetSingleStrategy()
        {
            var config = new TelemetryModuleConfiguration
            {
                WriterDiscoveryStrategy = TelemetryWriterDiscoveryStrategy.UseAllWritersInExplicitAssemblies
            };

            Assert.IsTrue(config.WriterDiscoveryStrategy.HasFlag(TelemetryWriterDiscoveryStrategy.UseAllWritersInExplicitAssemblies));
        }

        [TestMethod]
        public void CanSetMultipleStrategies()
        {
            var config = new TelemetryModuleConfiguration
            {
                WriterDiscoveryStrategy = TelemetryWriterDiscoveryStrategy.UseAllWritersInExplicitAssemblies | TelemetryWriterDiscoveryStrategy.UseExplicitlyDeclaredInstances
            };

            Assert.IsTrue(config.WriterDiscoveryStrategy.HasFlag(TelemetryWriterDiscoveryStrategy.UseAllWritersInExplicitAssemblies));
            Assert.IsTrue(config.WriterDiscoveryStrategy.HasFlag(TelemetryWriterDiscoveryStrategy.UseExplicitlyDeclaredInstances));
        }
    }
}