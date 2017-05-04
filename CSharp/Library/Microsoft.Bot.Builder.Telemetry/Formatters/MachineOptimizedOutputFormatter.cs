using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry.Data;
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

        public string FormatServiceResult(IServiceResultTelemetry serviceResultTelemetry)
        {
            var record = new AggregatedTelemetryRecord
            {
                RecordType = "serviceResult",
                ServiceResultName = serviceResultTelemetry.ServiceResultName,
                ServiceResultResponse = serviceResultTelemetry.ServiceResultResponse,
                ServiceResultSuccess = serviceResultTelemetry.ServiceResultSuccess,
                ServiceResultMillisecondsDuration = serviceResultTelemetry.ServiceResultEndDateTime.Subtract(serviceResultTelemetry.ServiceResultStartDateTime).TotalMilliseconds
            };

            return record.AsStringWith(_context);
        }

        public string FormatIntent(IIntentTelemetry intentTelemetry)
        {
            var record = new AggregatedTelemetryRecord { RecordType = "intent", IntentName = intentTelemetry.IntentName, IntentText = intentTelemetry.IntentText, IntentScore = intentTelemetry.IntentScore };
            return record.AsStringWith(_context);
        }

        public string FormatEntity(IEntityTelemetry entityTelemetry)
        {
            var record = new AggregatedTelemetryRecord { RecordType = "entity", EntityType = entityTelemetry.EntityType, EntityValue = entityTelemetry.EntityValue };
            return record.AsStringWith(_context);
        }

        public string FormatResponse(IResponseTelemetry responseTelemetry)
        {

            var duration = responseTelemetry.ResponseStartTime.Subtract(responseTelemetry.ResponseEndDateTime).TotalMilliseconds;
            var record = new AggregatedTelemetryRecord
            {
                RecordType = "response",
                ResponseText = responseTelemetry.ResponseText,
                ResponseImageUrl = responseTelemetry.ResponseImageUrl,
                ResponseJson = responseTelemetry.ResponseJson,
                ResponseResult = responseTelemetry.ResponseResult,
                ResponseMillisecondsDuration = duration,
                ResponseIsCacheHit = responseTelemetry.ResponseIsCacheHit
            };

            return record.AsStringWith(_context);
        }

        public string FormatCounter(ICounterTelemetry counterTelemetry)
        {
            var record = new AggregatedTelemetryRecord { RecordType = "counter", CounterName = counterTelemetry.CounterName, CounterValue = counterTelemetry.CounterValue };
            return record.AsStringWith(_context);
        }

        public string FormatException(IExceptionTelemetry exceptionTelemetry)
        {
            var record = new AggregatedTelemetryRecord
            {
                RecordType = "exception",
                ExceptionContext = exceptionTelemetry.ExceptionContext,
                ExceptionComponent = exceptionTelemetry.ExceptionComponent,
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

            var record = new AggregatedTelemetryRecord { RecordType = "trace", TraceName = "trace", TraceJson = $"{jsonObject.ToString(Formatting.None)}" };
            return record.AsStringWith(_context);
        }
    }
}