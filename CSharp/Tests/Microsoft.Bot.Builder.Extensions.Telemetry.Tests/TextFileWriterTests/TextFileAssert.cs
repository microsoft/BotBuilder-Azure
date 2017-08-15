using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Tests.TextFileWriterTests
{
    public class TextFileAssert
    {
        public static void HasExpectedNumberOfLogEntries(string expectedFullyQualifiedFilePathName, int expectedEntryCount)
        {
            if (!File.Exists(expectedFullyQualifiedFilePathName))
            {
                throw new AssertFailedException($"Expected Log File {expectedFullyQualifiedFilePathName} not found.");
            }

            var actualEntryCount = 0;
            using (var reader = File.OpenText(expectedFullyQualifiedFilePathName))
            {
                while (reader.ReadLine() != null)
                {
                    actualEntryCount++;
                }
            }

            if (actualEntryCount != expectedEntryCount)
            {
                throw new AssertFailedException($"Expected {expectedEntryCount} entries in log file, found {actualEntryCount}.");
            }

        }
    }
}