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
    public class TelemetryWriterTypeRegistrationTests
    {
        private ContainerBuilder _containerBuilder;

        [TestInitialize]
        public void SetUp()
        {
            _containerBuilder = new ContainerBuilder();
        }


        [TestMethod]
        public void CanRegisterSingleTelemetryWriterType()
        {
            var config = new TelemetryModuleConfiguration();
            config.TelemetryWriterTypes.Add(typeof(TextFileTelemetryWriter));

            //register necessary dependencies
            _containerBuilder.RegisterType<TextFileTelemetryWriterConfiguration>();
            _containerBuilder.RegisterType<ShardPerDayStrategy>().As<IShardStrategy>();

            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsInstanceOfType(container.Resolve<ITelemetryWriter>(), typeof(TextFileTelemetryWriter), "ITelemetryWriter returned by the container is not the expected type.");
        }

        [TestMethod]
        public void CanRegisterMultipleTelemetryWriterTypes()
        {
            var config = new TelemetryModuleConfiguration();
            config.TelemetryWriterTypes.Add(typeof(TextFileTelemetryWriter));
            config.TelemetryWriterTypes.Add(typeof(DebugWindowTelemetryWriter));

            //register necessary dependencies
            _containerBuilder.RegisterType<TextFileTelemetryWriterConfiguration>();
            _containerBuilder.RegisterType<DebugWindowTelemetryWriterConfiguration>();
            _containerBuilder.RegisterType<ShardPerDayStrategy>().As<IShardStrategy>();



            _containerBuilder.RegisterModule(new TelemetryModule(config));
            var container = _containerBuilder.Build();

            Assert.IsTrue(container.Resolve<IList<ITelemetryWriter>>().Any(tw => tw.GetType() == typeof(TextFileTelemetryWriter)));
            Assert.IsTrue(container.Resolve<IList<ITelemetryWriter>>().Any(tw => tw.GetType() == typeof(DebugWindowTelemetryWriter)));

        }

    }
}