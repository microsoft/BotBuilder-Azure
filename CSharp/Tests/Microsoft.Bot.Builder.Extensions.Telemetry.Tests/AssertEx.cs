using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Tests
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


        public static void Throws<TExpectedException>(Action action) where TExpectedException : Exception
        {
            try
            {
                action.Invoke();
            }
            catch (TExpectedException)
            {
                //silently swallow this, since its expected
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected Exception of type {typeof(TExpectedException)}, got {e}");
            }
        }
    }
}