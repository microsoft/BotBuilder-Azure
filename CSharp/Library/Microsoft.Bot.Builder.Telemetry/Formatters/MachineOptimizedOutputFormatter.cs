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

        public string FormatServiceResult(ServiceResultTelemetry serviceResultTelemetry)
        {
            var record = new SingleRowTelemetryRecord
            {
                RecordType = "serviceResult",
                ServiceResultName = serviceResultTelemetry.ServiceName,
                ServiceResultResponse = serviceResultTelemetry.Result,
                ServiceResultSuccess = $"{serviceResultTelemetry.Success}",
                ServiceResultMilliseconds = $"{serviceResultTelemetry.EndDateTime.Subtract(serviceResultTelemetry.StartDateTime).TotalMilliseconds}"
            };

            return record.AsStringWith(_context);
        }

        public string FormatIntent(IntentTelemetry intentTelemetry)
        {
            var record = new SingleRowTelemetryRecord { RecordType = "intent", IntentName = intentTelemetry.Intent, IntentText = intentTelemetry.Text, IntentScore = $"{intentTelemetry.Score}" };
            return record.AsStringWith(_context);
        }

        public string FormatEntity(EntityTelemetry entityTelemetry)
        {
            var record = new SingleRowTelemetryRecord { RecordType = "entity", EntityType = entityTelemetry.Kind, EntityValue = entityTelemetry.Value };
            return record.AsStringWith(_context);
        }

        public string FormatResponse(ResponseTelemetry responseTelemetry)
        {

            var duration = responseTelemetry.StartTime.Subtract(responseTelemetry.EndDateTime).TotalMilliseconds;
            var record = new SingleRowTelemetryRecord
            {
                RecordType = "response",
                ResponseText = responseTelemetry.Text,
                ResponseImageUrl = responseTelemetry.ImageUrl,
                ResponseJson = responseTelemetry.Json,
                ResponseResult = responseTelemetry.Result,
                ResponseDuration = $"{duration}",
                ResponseCacheHit = $"{responseTelemetry.IsCacheHit}"
            };

            return record.AsStringWith(_context);
        }

        public string FormatCounter(CounterTelemetry counterTelemetry)
        {
            var record = new SingleRowTelemetryRecord { RecordType = "counter", CounterName = counterTelemetry.Counter, CounterValue = $"{counterTelemetry.Count}" };
            return record.AsStringWith(_context);
        }

        public string FormatException(ExceptionTelemetry exceptionTelemetry)
        {
            var record = new SingleRowTelemetryRecord
            {
                RecordType = "exception",
                ExceptionContext = exceptionTelemetry.Context,
                ExceptionComponent = exceptionTelemetry.Component,
                ExceptionMessage = exceptionTelemetry.Ex.Message,
                ExceptionDetail = exceptionTelemetry.Ex.ToString(),
                ExceptionType = exceptionTelemetry.Ex.GetType().ToString()
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