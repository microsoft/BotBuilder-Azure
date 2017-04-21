using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests.DebugWriterTests
{
    [TestClass]
    public class WhenInvokedInParallel : DebugTracingTestBase
    {
        [TestMethod]
        public async Task CanWriteTracingDataToDebugOutput()
        {
            await WriteTracingDataToDebugAndAssert(10, true);
        }
    }
}