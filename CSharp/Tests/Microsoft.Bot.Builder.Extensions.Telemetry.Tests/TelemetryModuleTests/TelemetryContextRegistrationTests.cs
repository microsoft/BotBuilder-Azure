using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Tests.TelemetryModuleTests
{
    [TestClass]
    public class TelemetryContextRegistrationTests
    {
        private ContainerBuilder _containerBuilder;

        [TestInitialize]
        public void SetUp()
        {
            _containerBuilder = new ContainerBuilder();
        }

        [TestMethod]
        public void CanResolveTelemetryContext()
        {
            _containerBuilder.RegisterModule(new TelemetryModule(new TelemetryModuleConfiguration()));
            var container = _containerBuilder.Build();

            AssertEx.DoesNotThrow(() => container.Resolve<ITelemetryContext>());
        }
    }
}