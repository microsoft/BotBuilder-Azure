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
                WriterDiscoveryStrategy = TelemetryWriterDiscoveryStrategy.ScanExplicitFileSystemLocation
            };

            Assert.IsFalse(config.WriterDiscoveryStrategy.HasFlag(TelemetryWriterDiscoveryStrategy.ScanAssemblyFileSystemLocation));
        }

        [TestMethod]
        public void CanSetMultipleStrategies()
        {
            var config = new TelemetryModuleConfiguration
            {
                WriterDiscoveryStrategy = TelemetryWriterDiscoveryStrategy.ScanExplicitFileSystemLocation | TelemetryWriterDiscoveryStrategy.UseExplicitlyDeclaredInstances
            };

            Assert.IsTrue(config.WriterDiscoveryStrategy.HasFlag(TelemetryWriterDiscoveryStrategy.ScanExplicitFileSystemLocation));
            Assert.IsTrue(config.WriterDiscoveryStrategy.HasFlag(TelemetryWriterDiscoveryStrategy.UseExplicitlyDeclaredInstances));
        }
    }
}