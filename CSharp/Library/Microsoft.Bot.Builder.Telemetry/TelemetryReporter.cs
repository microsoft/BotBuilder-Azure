using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Data;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class TelemetryReporter : ITelemetryReporter
    {
        public TelemetryReporter(IEnumerable<ITelemetryWriter> writers, TelemetryReporterConfiguration configuration = null)
        {
            Configuration = configuration ?? new TelemetryReporterConfiguration();
            TelemetryWriters = new List<ITelemetryWriter>(writers);
        }

        public TelemetryReporterConfiguration Configuration { get; }

        public List<ITelemetryWriter> TelemetryWriters { get; set; }

        public async Task ReportIntentAsync(IIntentTelemetry intentTelemetry)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue intent
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteIntentAsync(intentTelemetry)); });
                //enqueue every entity
                TelemetryWriters.ForEach(tw => { tasks.AddRange(ProcessEntities(intentTelemetry.IntentEntities)); });

                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }

        public async Task ReportResponseAsync(IResponseTelemetry responseTelemetry)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue all tasks
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteResponseAsync(responseTelemetry)); });
                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }

        public async Task SetContextFrom(IActivity activity, ITelemetryContext context = null)
        {
            if (null == context)
            {
                context = new TelemetryContext(new DateTimeProvider());
            }

            context.ActivityId = activity.Id;
            context.ChannelId = activity.ChannelId;
            context.ConversationId = activity.Conversation.Id;
            context.UserId = activity.Conversation.Name;

            //flow the context through to all the children objects which depend upon it
            SetContext(context);
        }

        private List<Task> ProcessEntities(Dictionary<string, string> entities)
        {
            List<Task> tasks = new List<Task>();
            foreach (var entity in entities)
            {
                TelemetryWriters.ForEach(
                    tw =>
                    {
                        tasks.Add(
                            tw.WriteEntityAsync(new SingleRowTelemetryRecord
                            {
                                EntityType = entity.Key,
                                EntityValue = entity.Value
                            }));
                    });
            }

            return tasks;
        }

        public async Task ReportServiceResultAsync(IServiceResultTelemetry serviceResultTelemetry)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue all tasks
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteServiceResultAsync(serviceResultTelemetry)); });
                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }

        public void AddRequestProcessor(ITelemetryWriter processor)
        {
            TelemetryWriters.Add(processor);
        }

        public void RemoveRequestProcessor(ITelemetryWriter processor)
        {
            TelemetryWriters.Remove(processor);
        }

        public void RemoveRequestAllProcessors()
        {
            TelemetryWriters.Clear();
        }

        public async Task ReportDialogImpressionAsync(string dialog)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue all tasks
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteCounterAsync(new SingleRowTelemetryRecord { CounterName = dialog, CounterValue = 1 })); });
                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }

        public void SetContext(ITelemetryContext context)
        {
            TelemetryWriters.ForEach(tw => tw.SetContext(context));
        }

        public async Task ReportEventAsync(string key, string value)
        {
            await ReportEventAsync(new Dictionary<string, string> { { key, value } });
        }

        public async Task ReportEventAsync(string key, double value)
        {
            await ReportEventAsync(new Dictionary<string, double> { { key, value } });
        }

        public async Task ReportEventAsync(Dictionary<string, double> metrics)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue intent
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteEventAsync(metrics)); });

                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }

        public async Task ReportEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue intent
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteEventAsync(properties, metrics)); });

                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }
    }
}