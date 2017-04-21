using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests.TextFileWriterTests
{
    [TestClass]
    public class WhenInvokedInSequence : TextFileTelemetryTestBase
    {

        [TestMethod]
        //[Parallelizable(ParallelScope.None)] //important that the TestCases don't themselves run in parallel, else will stomp on the stream's lock on the FlatFile
        public async Task CanWriteTracingDataToFile()
        {
            await WriteTracingDataToFileAndAssert(10, false);
        }
    }
}