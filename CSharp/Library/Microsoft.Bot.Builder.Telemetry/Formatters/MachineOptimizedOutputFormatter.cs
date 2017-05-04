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

        public string FormatServiceResult(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            var record = new TelemetryData
            {
                RecordType = "serviceResult",
                ServiceResultName = serviceResultTelemetryData.ServiceResultName,
                ServiceResultResponse = serviceResultTelemetryData.ServiceResultResponse,
                ServiceResultSuccess = serviceResultTelemetryData.ServiceResultSuccess,
                ServiceResultStartDateTime = serviceResultTelemetryData.ServiceResultStartDateTime,
                ServiceResultEndDateTime = serviceResultTelemetryData.ServiceResultEndDateTime,
            };

            return record.AsStringWith(_context);
        }

        public string FormatIntent(IIntentTelemetryData intentTelemetryData)
        {
            var record = new TelemetryData
            {
                RecordType = "intent",
                IntentName = intentTelemetryData.IntentName,
                IntentText = intentTelemetryData.IntentText,
                IntentScore = intentTelemetryData.IntentScore
            };
            return record.AsStringWith(_context);
        }

        public string FormatEntity(IEntityTelemetryData entityTelemetryData)
        {
            var record = new TelemetryData
            {
                RecordType = "entity",
                EntityType = entityTelemetryData.EntityType,
                EntityValue = entityTelemetryData.EntityValue,
                EntityConfidenceScore = entityTelemetryData.EntityConfidenceScore
            };
            return record.AsStringWith(_context);
        }

        public string FormatResponse(IResponseTelemetryData responseTelemetryData)
        {
            var record = new TelemetryData
            {
                RecordType = "response",
                ResponseText = responseTelemetryData.ResponseText,
                ResponseImageUrl = responseTelemetryData.ResponseImageUrl,
                ResponseJson = responseTelemetryData.ResponseJson,
                ResponseResult = responseTelemetryData.ResponseResult,
                ResponseType = responseTelemetryData.ResponseType,
                ResponseStartDateTime = responseTelemetryData.ResponseStartDateTime,
                ResponseEndDateTime = responseTelemetryData.ResponseEndDateTime,
                ResponseIsCacheHit = responseTelemetryData.ResponseIsCacheHit
            };

            return record.AsStringWith(_context);
        }

        public string FormatCounter(ICounterTelemetryData counterTelemetryData)
        {
            var record = new TelemetryData
            {
                RecordType = "counter",
                CounterName = counterTelemetryData.CounterName,
                CounterValue = counterTelemetryData.CounterValue
            };
            return record.AsStringWith(_context);
        }

        public string FormatException(IExceptionTelemetryData exceptionTelemetryData)
        {
            var record = new TelemetryData
            {
                RecordType = "exception",
                ExceptionContext = exceptionTelemetryData.ExceptionContext,
                ExceptionComponent = exceptionTelemetryData.ExceptionComponent,
                Ex = exceptionTelemetryData.Ex,
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

            var record = new TelemetryData { RecordType = "trace", TraceName = "trace", TraceJson = $"{jsonObject.ToString(Formatting.None)}" };
            return record.AsStringWith(_context);
        }
    }
}