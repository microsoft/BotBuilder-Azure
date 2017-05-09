using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Microsoft.Bot.Builder.Telemetry.DebugWriter;
using Microsoft.Bot.Builder.Telemetry.Formatters;
using Microsoft.Bot.Builder.Telemetry.TextFileWriter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests.TelemetryModuleTests
{
    [TestClass]
    public class TelemetryWriterAssemblyRegistrationTests
    {
        private ContainerBuilder _containerBuilder;

        [TestInitialize]
        public void SetUp()
        {
            _containerBuilder = new ContainerBuilder();
        }


        [TestMethod]
        public void CanRegisterSingleTelemetryWriterFromAssembly()
        {
            var config = new TelemetryModuleConfiguration();
            config.TelemetryWriterAssemblies.Add(typeof(TextFileTelemetryWriter).Assembly);
            config.TelemetryWriterConfigurations.Add(new TextFileTelemetryWriterConfiguration(new PerDayShardStrategy()));

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsInstanceOfType(container.Resolve<ITelemetryWriter>(), typeof(TextFileTelemetryWriter), "ITelemetryWriter returned by the container is not the expected type.");
        }

        [TestMethod]
        public void CanRegisterTelemetryWritersFromMultipleAssemblies()
        {
            var config = new TelemetryModuleConfiguration();

            config.TelemetryWriterAssemblies.Add(typeof(TextFileTelemetryWriter).Assembly);
            config.TelemetryWriterAssemblies.Add(typeof(DebugWindowTelemetryWriter).Assembly);

            config.TelemetryWriterConfigurations.Add(new TextFileTelemetryWriterConfiguration(new PerDayShardStrategy()));
            config.TelemetryWriterConfigurations.Add(new DebugWindowTelemetryWriterConfiguration());

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsTrue(container.Resolve<IList<ITelemetryWriter>>().Any(obj => obj is TextFileTelemetryWriter));
            Assert.IsTrue(container.Resolve<IList<ITelemetryWriter>>().Any(obj => obj is DebugWindowTelemetryWriter));
        }

    }
}