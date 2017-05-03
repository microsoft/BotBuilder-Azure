using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry.Tests
{
    public abstract class TelemetryIntegrationTestsBase
    {
        public class TestParameters
        {
            private string _uniqueTestRunId;
            public int RowCount { get; set; }
            public bool Concurrent { get; set; }

            public string UniqueTestRunId
            {
                get
                {
                    if (string.IsNullOrEmpty(_uniqueTestRunId))
                    {
                        _uniqueTestRunId = Guid.NewGuid().ToString();
                    }

                    return _uniqueTestRunId;
                }
                set
                {
                    _uniqueTestRunId = value;
                }
            }
        }

        protected virtual ITelemetryContext BuildTestTracingContext()
        {
            return new TelemetryContext(new DateTimeProvider())
            {
                ChannelId = "The_ChannelId",
                ActivityId = "The_ActivityId",
                ConversationId = "The_ConversationId",
                UserId = "The_UserId"
            };
        }

        protected async Task WriteTracingData(int rowCount, bool concurrent, string uniqueTestRunId)
        {
            ITelemetryWriter telemetryWriter = GetTelemetryWriter();

            if (concurrent)
            {
                var tasks = new List<Task>();

                Parallel.For(0, rowCount, unusedCounter =>
                {
                    tasks.Add(BasicExerciseOfRequestProcessor(telemetryWriter, uniqueTestRunId));
                });

                //since we launched in parallel, have to await all tasks else test will exit before fire-and-forget write tasks
                // actually get a chance to complete
                Task.WaitAll(tasks.Where(t => null != t).ToArray());
            }
            else
            {
                for (var i = 0; i < rowCount; i++)
                {
                    await BasicExerciseOfRequestProcessor(telemetryWriter, uniqueTestRunId);

                }
            }
        }

        protected abstract ITelemetryWriter GetTelemetryWriter();

        //this CONST must be remain in sync with the body of the BasicExerciseOfRequestProcessor(...) method
        protected const int LogEntriesExpectedPerTestCaseRun = 6;

        protected async Task BasicExerciseOfRequestProcessor(ITelemetryWriter processor, string uniqueTestRunId)
        {
            //we can intercept the debug stream, but for now this 
            // will be very simple, we call all methods and ensure that there is no exceptions
            await processor.WriteCounterAsync($"counterNameValue ({uniqueTestRunId})");
            await processor.WriteExceptionAsync($"componentName ({uniqueTestRunId})", "component context", new Exception("this is a bad exception"));
            await processor.WriteServiceResultAsync($"serviceNameValue ({uniqueTestRunId})", DateTime.Now, DateTime.Now.AddSeconds(10), "HTTP:200", true);
            await processor.WriteEntityAsync($"kindNameValue ({uniqueTestRunId})", "value");
            await processor.WriteIntentAsync($"IntentNameValue ({uniqueTestRunId})", "intent text", 0.5f);
            await processor.WriteResponseAsync($"ResponseText ({uniqueTestRunId})", "imageUrl", "{someProperty: \"some json value\"}", "this is a result", false);
        }
    }
}
