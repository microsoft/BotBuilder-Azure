using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests
{
    [TestClass]
    public class TelemetryReporterTests
    {
        [TestMethod]
        public void ConfigurationDefaultIsFailSilently()
        {
            var config = new TelemetryReporterConfiguration();
            Assert.IsTrue(config.FailSilently);
        }

        [TestMethod]
        public void CanSwallowExceptionsByDefault()
        {
            var reporter = new TelemetryReporter(new[] { new ExceptionThrowingTelemetryWriter() });
            AssertEx.DoesNotThrow(() => reporter.ReportRequestAsync(TelemetryData.NewRequestData()).GetAwaiter().GetResult());
        }

        [TestMethod]
        public void CanThrowExceptionsWhenFailSilentlyIsFalse()
        {
            var config = new TelemetryReporterConfiguration { FailSilently = false };
            var reporter = new TelemetryReporter(new[] { new ExceptionThrowingTelemetryWriter() }, config);
            AssertEx.Throws<TelemetryException>(() => reporter.ReportRequestAsync(TelemetryData.NewRequestData()).GetAwaiter().GetResult());
        }

        internal class ExceptionThrowingTelemetryWriter : ITelemetryWriter
        {
            public void SetContext(ITelemetryContext context)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteEventAsync(string key, string value)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteEventAsync(string key, double value)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteEventAsync(Dictionary<string, double> metrics)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
            {
                throw new System.NotImplementedException();
            }

            public async Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}