using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Extensions.Telemetry
{
    public class TelemetryReporter : ITelemetryReporter
    {
        public TelemetryReporterConfiguration Configuration { get; }
        public List<ITelemetryWriter> TelemetryWriters { get; set; }

        public TelemetryReporter(IEnumerable<ITelemetryWriter> writers, TelemetryReporterConfiguration configuration = null)
        {
            Configuration = configuration ?? new TelemetryReporterConfiguration();
            TelemetryWriters = new List<ITelemetryWriter>(writers);
        }

        public async Task ReportIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteIntentAsync(intentTelemetryData)); });
                TelemetryWriters.ForEach(tw => { tasks.AddRange(ProcessEntities(intentTelemetryData.IntentEntities)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }

        public async Task ReportRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteRequestAsync(requestTelemetryData)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }

        public async Task ReportResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteResponseAsync(responseTelemetryData)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }


        public async Task ReportExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteExceptionAsync(exceptionTelemetryData)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }

        public Task SetContextFrom(IActivity activity, ITelemetryContext context = null)
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

            return Task.Delay(0);
        }

        private IEnumerable<Task> ProcessEntities(IEnumerable<IEntityTelemetryData> entities)
        {
            var tasks = new List<Task>();
            foreach (var entity in entities)
            {
                TelemetryWriters.ForEach(tw => tasks.Add(tw.WriteEntityAsync(entity)));
            }
            return tasks;
        }

        public async Task ReportServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteServiceResultAsync(serviceResultTelemetryData)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }

        public void AddTelemetryWriter(ITelemetryWriter telemetryWriter)
        {
            TelemetryWriters.Add(telemetryWriter);
        }

        public void RemoveTelemetryWriter(ITelemetryWriter telemetryWriter)
        {
            TelemetryWriters.Remove(telemetryWriter);
        }

        public void RemoveAllTelemetryWriters()
        {
            TelemetryWriters.Clear();
        }

        public async Task ReportDialogImpressionAsync(string dialog)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteCounterAsync(new TelemetryData { CounterName = dialog, CounterValue = 1 })); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
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
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteEventAsync(metrics)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }

        public async Task ReportEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            try
            {
                var tasks = new List<Task>();
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteEventAsync(properties, metrics)); });

                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                if (!Configuration.FailSilently)
                {
                    throw new TelemetryException("Failed to write to TelemetryWriters.", e);
                }
            }
        }
    }
}