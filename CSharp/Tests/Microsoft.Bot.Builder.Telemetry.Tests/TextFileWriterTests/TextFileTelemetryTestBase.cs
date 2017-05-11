using System.IO;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Formatters;
using Microsoft.Bot.Builder.Telemetry.TextFileWriter;

namespace Microsoft.Bot.Builder.Telemetry.Tests.TextFileWriterTests
{
    public abstract class TextFileTelemetryTestBase : TelemetryIntegrationTestsBase
    {
        //ridiculous work-around for MSTEST inability to properly execute [ClassInitialize]-attributed method
        // so we have to 'hide' the behavior in the ctor
        protected TextFileTelemetryTestBase()
        {
            OneTimeSetUp();
        }

        protected void OneTimeSetUp()
        {
            var logFile = new TextFileTelemetryWriterConfiguration(new ShardPerDayStrategy()).Filename;

            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }

        public async Task WriteTracingDataToFileAndAssert(int rowCount, bool concurrentWriters)
        {
            var testParams = new TestParameters {Concurrent = concurrentWriters, RowCount = rowCount};

            await WriteTracingData(testParams.RowCount, testParams.Concurrent, testParams.UniqueTestRunId);

            var expectedEntryCount = LogEntriesExpectedPerTestCaseRun * testParams.RowCount;
            TextFileAssert.HasExpectedNumberOfLogEntries(new TextFileTelemetryWriterConfiguration(new ShardPerDayStrategy()).Filename, expectedEntryCount);
        }

        protected override ITelemetryWriter GetTelemetryWriter()
        {
            var config = new TextFileTelemetryWriterConfiguration(new ShardPerDayStrategy());
            var context = BuildTestTracingContext();
            var formatter = new MachineOptimizedOutputFormatter(context);

            return new TextFileTelemetryWriter(config, formatter);
        }
    }
}