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


        public static void Throws<TException>(Action action) where TException : Exception
        {
            try
            {
                action.Invoke();
            }
            catch (TException)
            {
                //silently swallow this, since its expected
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected Exception of type {typeof(TException)}, got {e}");
            }
        }
    }
}