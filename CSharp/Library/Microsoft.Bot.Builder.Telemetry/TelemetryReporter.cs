using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class TelemetryReporter : ITelemetryReporter
    {
        public TelemetryReporter(IEnumerable<ITelemetryWriter> processors, TelemetryReporterConfiguration configuration = null)
        {
            Configuration = configuration ?? new TelemetryReporterConfiguration();
            TelemetryWriters = new List<ITelemetryWriter>(processors);
        }

        public TelemetryReporterConfiguration Configuration { get; }

        public List<ITelemetryWriter> TelemetryWriters { get; set; }

        public async Task AddLuisEventDetailsAsync(string intent, float confidence, Dictionary<string, string> entities)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue intent
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteIntentAsync(intent, confidence)); });
                //enqueue every entity
                TelemetryWriters.ForEach(tw => { tasks.AddRange(ProcessEntities(entities)); });

                //await all in parallel.
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                //We will write this into a debug window as the logging into Telemetry Writers may fail also.
                throw new TelemetryException("Failed to write to TelemetryWriters.", e);
            }
        }

        private List<Task> ProcessEntities(Dictionary<string, string> entities)
        {
            List<Task> tasks = new List<Task>();
            foreach (var entity in entities)
            {
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteEntityAsync(entity.Key, entity.Value)); });
            }

            return tasks;
        }

        public async Task AddServiceResultAsync(string serviceName, DateTime startTime, DateTime endTime, string result, bool success = true)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue all tasks
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteServiceResultAsync(serviceName, startTime, endTime, result, success)); });
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

        public async Task AddDialogImpressionAsync(string dialog)
        {
            try
            {
                var tasks = new List<Task>();
                //enqueue all tasks
                TelemetryWriters.ForEach(tw => { tasks.Add(tw.WriteCounterAsync(dialog)); });
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

        public async Task AddEventAsync(string key, string value)
        {
            await AddEventAsync(new Dictionary<string, string> { { key, value } });
        }

        public async Task AddEventAsync(string key, double value)
        {
            await AddEventAsync(new Dictionary<string, double> { { key, value } });
        }

        public async Task AddEventAsync(Dictionary<string, double> metrics)
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

        public async Task AddEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
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