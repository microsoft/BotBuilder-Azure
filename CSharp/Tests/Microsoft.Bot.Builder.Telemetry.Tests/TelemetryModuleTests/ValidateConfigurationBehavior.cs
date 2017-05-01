using Autofac;
using Microsoft.Bot.Builder.Telemetry.DebugWriter;
using Microsoft.Bot.Builder.Telemetry.TextFileWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests.TelemetryModuleTests
{
    [TestClass]
    public class ValidateConfigurationBehavior
    {
        private ContainerBuilder _containerBuilder;

        [TestInitialize]
        public void SetUp()
        {
            _containerBuilder = new ContainerBuilder();
        }


        [TestMethod]
        public void CanRegisterSingleProvidedWriterConfiguration()
        {
            var config = new TelemetryModuleConfiguration();
            config.TelemetryWriterConfigurations.Add(new TextFileTelemetryWriterConfiguration(new PerDayShardStrategy()));

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsTrue(container.IsRegistered<TextFileTelemetryWriterConfiguration>());
        }

        [TestMethod]
        public void CanRegisterMultipleProvidedWriterConfigurations()
        {
            var config = new TelemetryModuleConfiguration();
            config.TelemetryWriterConfigurations.Add(new TextFileTelemetryWriterConfiguration(new PerDayShardStrategy()));
            config.TelemetryWriterConfigurations.Add(new DebugWindowTelemetryWriterConfiguration());

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsTrue(container.IsRegistered<TextFileTelemetryWriterConfiguration>());
            Assert.IsTrue(container.IsRegistered<DebugWindowTelemetryWriterConfiguration>());
        }
    }
}