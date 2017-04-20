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

        public async Task WriteIntentAsync(string intent, float score, Dictionary<string, string> entities = null)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();
                    properties.Add("intent", intent);

                    var metrics = new Dictionary<string, double>
                    {
                        {"score", score }
                    };

                    _telemetry.TrackEvent("Intent", properties, metrics);
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteEntityAsync(string kind, string value)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("entity", kind);
                    properties.Add("value", value);

                    _telemetry.TrackEvent("Entity", properties);
                    DoPostLogActions();

                });
            }
        }

        public async Task WriteCounterAsync(string counter, int count = 1)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();
                    properties.Add("name", counter);

                    var metrics = new Dictionary<string, double> { { "count", count } };

                    _telemetry.TrackEvent("Counter", properties, metrics);
                    DoPostLogActions();
                });
            }
        }

        public async Task WriteServiceResultAsync(string serviceName, DateTime startTime, DateTime endDateTime, string result, bool success = true)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("serviceName", serviceName);
                    properties.Add("result", result);
                    properties.Add("success", success.ToString());

                    var metrics = new Dictionary<string, double>
                    {
                        {"millisecondsDuration", endDateTime.Subtract(startTime).TotalMilliseconds }
                    };

                    _telemetry.TrackEvent("ServiceResult", properties, metrics);
                    DoPostLogActions();
                });
            }

        }

        public async Task WriteExceptionAsync(string component, string context, Exception e)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("component", component);
                    properties.Add("context", context);

                    _telemetry.TrackException(e, properties);
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