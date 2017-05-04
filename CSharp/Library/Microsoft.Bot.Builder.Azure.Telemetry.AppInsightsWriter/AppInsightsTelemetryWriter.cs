using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Telemetry.AppInsightsWriter
{
    public class AppInsightsTelemetryWriter : ITelemetryWriter
    {
        private ITelemetryContext _context;
        private TelemetryClient _telemetry;
        private TelemetryConfiguration _telemetryConfiguration;
        private readonly AppInsightsTelemetryWriterConfiguration _configuration;

        public AppInsightsTelemetryWriter(AppInsightsTelemetryWriterConfiguration configuration, ITelemetryContext context)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out _context, nameof(context), context);

            Initialize();
        }

        private void Initialize()
        {
            //set initial configuration
            _telemetryConfiguration = new ApplicationInsights.Extensibility.TelemetryConfiguration() { };
            _telemetryConfiguration.InstrumentationKey = _configuration.InstrumentationKey;

            _telemetry = new TelemetryClient(_telemetryConfiguration)
            {
                //WARNING: despite this value having been set in the TelemetryConfiguration instance passed to the ctor of the
                // TelemetryClient (above), for some reason we have to manually (re)assign this as follows for it to be
                // properly picked up by the TelemetryClient (possible BUG in AppInsights --???)
                //TODO: figure out why we have to (re)assign this value directly for it to be respected by AppInsights
                InstrumentationKey = _telemetryConfiguration.InstrumentationKey
            };

#if DEBUG
            //forces the data to appear in the Azure portal in a more timely manner to accelerate the dev/test cycle
            TelemetryConfiguration.Active.TelemetryChannel.DeveloperMode = true;
#endif
        }

        private void DoPostLogActions()
        {
            if (_configuration.FlushEveryWrite) _telemetry.Flush();
        }

        public async Task WriteIntentAsync(IIntentTelemetry intentTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();
                    properties.Add("intent", intentTelemetry.IntentName);

                    var metrics = new Dictionary<string, double>
                    {
                        {"score", intentTelemetry.IntentScore }
                    };

                    _telemetry.TrackEvent("Intent", properties, metrics);
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteEntityAsync(IEntityTelemetry entityTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("entity", entityTelemetry.EntityType);
                    properties.Add("value", entityTelemetry.EntityValue);

                    _telemetry.TrackEvent("Entity", properties);
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteResponseAsync(IResponseTelemetry responseTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                var duration = responseTelemetry.ResponseStartTime.Subtract(responseTelemetry.ResponseEndDateTime).TotalMilliseconds;

                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("text", responseTelemetry.ResponseText);
                    properties.Add("image", responseTelemetry.ResponseImageUrl);
                    properties.Add("json", responseTelemetry.ResponseJson);
                    properties.Add("result", responseTelemetry.ResponseResult);
                    properties.Add("duration", $"{duration}");
                    properties.Add("cacheHit", $"{responseTelemetry.ResponseIsCacheHit}");

                    _telemetry.TrackEvent("Response", properties);
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteCounterAsync(ICounterTelemetry counterTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();
                    properties.Add("name", counterTelemetry.CounterName);

                    var metrics = new Dictionary<string, double> { { "count", counterTelemetry.CounterValue } };

                    _telemetry.TrackEvent("Counter", properties, metrics);
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteServiceResultAsync(IServiceResultTelemetry serviceResultTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("serviceName", serviceResultTelemetry.ServiceResultName);
                    properties.Add("result", serviceResultTelemetry.ServiceResultResponse);
                    properties.Add("success", serviceResultTelemetry.ServiceResultSuccess.ToString());

                    var metrics = new Dictionary<string, double>
                    {
                        {"millisecondsDuration", serviceResultTelemetry.ServiceResultEndDateTime.Subtract(serviceResultTelemetry.ServiceResultStartDateTime).TotalMilliseconds }
                    };

                    _telemetry.TrackEvent("ServiceResult", properties, metrics);
                    DoPostLogActions();
                });
            }

        }

        public async Task WriteExceptionAsync(IExceptionTelemetry exceptionTelemetry)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("component", exceptionTelemetry.ExceptionComponent);
                    properties.Add("context", exceptionTelemetry.ExceptionContext);

                    _telemetry.TrackException(exceptionTelemetry.Ex, properties);
                    DoPostLogActions();
                });
            }
        }

        public void SetContext(ITelemetryContext context)
        {
            _context = context;
        }

        private Dictionary<string, string> GetBotContextProperties()
        {
            return new Dictionary<string, string>
            {
                {"channelId", _context.ChannelId },
                {"conversationId", _context.ConversationId },
                {"activityId", _context.ActivityId },
                {"userId", _context.UserId },
                {"timestamp", _context.Timestamp },
                {"correlationId", _context.CorrelationId },
            };
        }
        public async Task WriteEventAsync(string key, string value)
        {
            await WriteEventAsync(new Dictionary<string, string> { { key, value } });
        }

        public async Task WriteEventAsync(string key, double value)
        {
            await WriteEventAsync(new Dictionary<string, double> { { key, value } });
        }

        public async Task WriteEventAsync(Dictionary<string, double> metrics)
        {
            await WriteEventAsync(GetBotContextProperties(), metrics);
        }

        public async Task WriteEventAsync(Dictionary<string, string> eventProperties, Dictionary<string, double> eventMetrics = null)
        {
            if (_configuration.Handles(TelemetryTypes.CustomEvents))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    foreach (var prop in eventProperties)
                    {
                        properties.Add(prop.Key, prop.Value);
                    }

                    if (null != eventMetrics)
                    {
                        var metrics = new Dictionary<string, double>();

                        foreach (var metric in eventMetrics)
                        {
                            metrics.Add(metric.Key, metric.Value);
                        }

                        _telemetry.TrackEvent("Trace", properties, metrics);

                    }
                    else
                    {
                        _telemetry.TrackEvent("Trace", properties);
                    }


                    DoPostLogActions();
                });
            }
        }
    }
}