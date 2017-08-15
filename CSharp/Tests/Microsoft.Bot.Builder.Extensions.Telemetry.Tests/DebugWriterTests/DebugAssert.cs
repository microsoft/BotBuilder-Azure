using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Tests.DebugWriterTests
{
    public class DebugAssert
    {
        public static void HasExpectedNumberOfLogEntries(IEnumerable<string> capturedMessages, int expectedEntryCount)
        {
            var actualEntryCount = capturedMessages.Count();
            if (actualEntryCount != expectedEntryCount)
            {
                throw new AssertFailedException($"Expected {expectedEntryCount} entries in log file, found {actualEntryCount}.");
            }
        }
    }
}