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
            serviceResultTelemetryData.RecordType = "serviceResult";
            return serviceResultTelemetryData.AsStringWith(_context);
        }

        public string FormatRequest(IRequestTelemetryData requestTelemetryData)
        {
            requestTelemetryData.RecordType = "request";
            return requestTelemetryData.AsStringWith(_context);
        }

        public string FormatIntent(IIntentTelemetryData intentTelemetryData)
        {
            intentTelemetryData.RecordType = "intent";
            return intentTelemetryData.AsStringWith(_context);
        }

        public string FormatEntity(IEntityTelemetryData entityTelemetryData)
        {
            entityTelemetryData.RecordType = "entity";
            return entityTelemetryData.AsStringWith(_context);
        }

        public string FormatResponse(IResponseTelemetryData responseTelemetryData)
        {
            responseTelemetryData.RecordType = "response";
            return responseTelemetryData.AsStringWith(_context);
        }

        public string FormatCounter(ICounterTelemetryData counterTelemetryData)
        {
            counterTelemetryData.RecordType = "counter";
            return counterTelemetryData.AsStringWith(_context);
        }

        public string FormatMeasure(IMeasureTelemetryData measureTelemetryData)
        {
            measureTelemetryData.RecordType = "measure";
            return measureTelemetryData.AsStringWith(_context);
        }

        public string FormatException(IExceptionTelemetryData exceptionTelemetryData)
        {
            exceptionTelemetryData.RecordType = "exception";
            return exceptionTelemetryData.AsStringWith(_context);
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