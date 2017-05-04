using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Data;

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
            await processor.WriteCounterAsync(new SingleRowTelemetryRecord { CounterName = $"counterNameValue ({uniqueTestRunId})" });
            await processor.WriteExceptionAsync(new SingleRowTelemetryRecord { ExceptionComponent = $"componentName ({uniqueTestRunId})", ExceptionContext = "component context", Ex = new Exception("this is a bad exception") });
            await processor.WriteServiceResultAsync(new SingleRowTelemetryRecord { ServiceResultName = $"serviceNameValue ({uniqueTestRunId})", ServiceResultStartDateTime = DateTime.Now, ServiceResultEndDateTime = DateTime.Now.AddSeconds(10), ServiceResultResponse = "HTTP:200", ServiceResultSuccess = true });
            await processor.WriteEntityAsync(new SingleRowTelemetryRecord { EntityType = $"kindNameValue ({uniqueTestRunId})", EntityValue = "value" });
            await processor.WriteIntentAsync(new SingleRowTelemetryRecord { IntentName = $"IntentNameValue ({uniqueTestRunId})", IntentText = "intent text", IntentScore = 0.5f });
            await processor.WriteResponseAsync(new SingleRowTelemetryRecord { ResponseText = $"ResponseText ({uniqueTestRunId})", ResponseImageUrl = "imageUrl", ResponseJson = "{someProperty: \"some json value\"}", ResponseResult = "this is a result", ResponseStartTime = DateTime.Now, ResponseEndDateTime = DateTime.Now.Add(TimeSpan.FromMilliseconds(100)), ResponseIsCacheHit = false });
        }
    }
}
