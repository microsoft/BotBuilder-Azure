using System.Collections;
using System.Collections.Generic;
using Autofac;
using Microsoft.Bot.Builder.Telemetry.DebugWriter;
using Microsoft.Bot.Builder.Telemetry.Formatters;
using Microsoft.Bot.Builder.Telemetry.TextFileWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests.TelemetryModuleTests
{
    [TestClass]
    public class TelemetryWriterInstanceRegistrationTests
    {
        private ContainerBuilder _containerBuilder;

        [TestInitialize]
        public void SetUp()
        {
            _containerBuilder = new ContainerBuilder();
        }


        [TestMethod]
        public void CanRegisterSingleTelemetryWriterInstance()
        {
            var config = new TelemetryModuleConfiguration();
            var textWriter = BuildDefaultTextWriter();
            config.TelemetryWriterInstances.Add(textWriter);

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsInstanceOfType(container.Resolve<ITelemetryWriter>(), typeof(TextFileTelemetryWriter),"ITelemetryWriter returned by the container is not the expected type.");
        }

        private static TextFileTelemetryWriter BuildDefaultTextWriter()
        {
            return new TextFileTelemetryWriter(
                new TextFileTelemetryWriterConfiguration(
                    new ShardPerDayStrategy()), 
                new MachineOptimizedOutputFormatter(new TelemetryContext(new DateTimeProvider())));
        }


        [TestMethod]
        public void CanRegisterMultipleTelemetryWriterInstances()
        {
            var config = new TelemetryModuleConfiguration();
            var textWriter = BuildDefaultTextWriter();

            var debugWriter = new DebugWindowTelemetryWriter(
                new DebugWindowTelemetryWriterConfiguration(),
                new ReadabilityOptimizedOutputFormatter(new TelemetryContext(new DateTimeProvider())));
            config.TelemetryWriterInstances.Add(debugWriter);
            config.TelemetryWriterInstances.Add(textWriter);

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsTrue(container.Resolve<IList<ITelemetryWriter>>().Contains(debugWriter));
            Assert.IsTrue(container.Resolve<IList<ITelemetryWriter>>().Contains(textWriter));

        }

    }
}