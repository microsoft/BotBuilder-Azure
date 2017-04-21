using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.DebugWriter;
using Microsoft.Bot.Builder.Telemetry.Formatters;

namespace Microsoft.Bot.Builder.Telemetry.Tests.DebugWriterTests
{
    public abstract class DebugTracingTestBase : TelemetryIntegrationTestsBase
    {
        public async Task WriteTracingDataToDebugAndAssert(int rowCount, bool concurrentWriters)
        {
            var listener = new MessageCapturingTraceListener();
            Trace.Listeners.Add(listener);

            var testParams = new TestParameters { Concurrent = concurrentWriters, RowCount = rowCount };

            await WriteTracingData(testParams.RowCount, testParams.Concurrent, testParams.UniqueTestRunId);

            //have to add 1 from LogEntriesExpectedPerTestCaseRun b/c the EXCEPTION message and the EXCEPTION itself
            // are reported as a separate entries in DebugWindow EXCEPTION reporting
            var expectedEntryCount = (LogEntriesExpectedPerTestCaseRun + 1) * testParams.RowCount;

            DebugAssert.HasExpectedNumberOfLogEntries(listener.CapturedMessages, expectedEntryCount);
        }

        protected override ITelemetryWriter GetTelemetryWriter()
        {
            var context = BuildTestTracingContext();
            var formatter = new ReadabilityOptimizedOutputFormatter(context);
            return new DebugWindowTelemetryWriter(new DebugWindowTelemetryWriterConfiguration(), formatter);
        }
    }
}