using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Telemetry.Tests
{
    public class AssertEx
    {
        public static void DoesNotThrow(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no Exception, got {e}");
            }
        }
    }
}