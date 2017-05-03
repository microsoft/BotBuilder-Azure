using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Internals.Fibers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Telemetry.Formatters
{
    public class MachineOptimizedOutputFormatter : ITelemetryOutputFormatter
    {
        private ITelemetryContext _context;


        public MachineOptimizedOutputFormatter(ITelemetryContext context)
        {
            SetField.NotNull(out _context, nameof(context), context);
        }

        public void SetContext(ITelemetryContext context)
        {
            _context = context;
        }

        public string FormatServiceResult(string serviceName, DateTime startTime, DateTime endTime, string result,
            bool success = true)
        {
            var record = new SingleRowTelemetryRecord
            {
                RecordType = "serviceResult",
                ServiceResultName = serviceName,
                ServiceResultResponse = result,
                ServiceResultSuccess = $"{success}",
                ServiceResultMilliseconds = $"{endTime.Subtract(startTime).TotalMilliseconds}"
            };

            return record.AsStringWith(_context);
        }

        public string FormatIntent(string intent, string text, double score)
        {
            var record = new SingleRowTelemetryRecord { RecordType = "intent", IntentName = intent, IntentText = text, IntentScore = $"{score}" };
            return record.AsStringWith(_context);
        }

        public string FormatEntity(string kind, string value)
        {
            var record = new SingleRowTelemetryRecord { RecordType = "entity", EntityType = kind, EntityValue = value };
            return record.AsStringWith(_context);
        }

        public string FormatResponse(string text, string imageUrl, string json, string result, DateTime startTime, DateTime endDateTime, bool isCacheHit)
        {

            var duration = startTime.Subtract(endDateTime).TotalMilliseconds;
            var record = new SingleRowTelemetryRecord
            {
                RecordType = "response",
                ResponseText = text,
                ResponseImage = imageUrl,
                ResponseJson = json,
                ResponseResult = result,
                ResponseDuration = $"{duration}",
                ResponseCacheHit = $"{isCacheHit}"
            };

            return record.AsStringWith(_context);
        }

        public string FormatCounter(string counter, int count)
        {
            var record = new SingleRowTelemetryRecord { RecordType = "counter", CounterName = counter, CounterValue = $"{count}" };
            return record.AsStringWith(_context);
        }

        public string FormatException(string component, string context, Exception ex)
        {
            var record = new SingleRowTelemetryRecord
            {
                RecordType = "exception",
                ExceptionContext = context,
                ExceptionComponent = component,
                ExceptionMessage = ex.Message,
                ExceptionDetail = ex.ToString(),
                ExceptionType = ex.GetType().ToString()
            };

            return record.AsStringWith(_context);
        }

        public string FormatEvent(Dictionary<string, double> metrics)
        {
            return FormatEvent(new Dictionary<string, string>(), metrics);
        }

        public string FormatEvent(Dictionary<string, string> properties, Dictionary<string, double> metrics = null)
        {
            //make sure we have at least an empty dictionary so that remainder of code needn't branch
            if (null == metrics)
            {
                metrics = new Dictionary<string, double>();
            }

            //build up JSON representation of the arbitrary dictionaries passed in
            var jsonObject = new JObject(

                    new JProperty("properties",
                        new JArray(
                            from p in properties
                            select new JObject(new JProperty(p.Key, p.Value))
                        )
                    ),

                    new JProperty("metrics",
                        new JArray(
                            from m in metrics
                            select new JObject(new JProperty(m.Key, m.Value)))
                    )

            );

            var record = new SingleRowTelemetryRecord { RecordType = "trace", TraceName = "trace", TraceValue = $"{jsonObject.ToString(Formatting.None)}" };
            return record.AsStringWith(_context);
        }
    }
}