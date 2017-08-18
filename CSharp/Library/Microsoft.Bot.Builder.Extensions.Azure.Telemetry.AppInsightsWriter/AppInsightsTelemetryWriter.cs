using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Extensions.Telemetry;
using Microsoft.Bot.Builder.Extensions.Telemetry.Data;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.AppInsightsWriter
{
    /// <summary>
    /// Class AppInsightsTelemetryWriter.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.ITelemetryWriter" />
    public class AppInsightsTelemetryWriter : ITelemetryWriter
    {
        /// <summary>
        /// The context
        /// </summary>
        private ITelemetryContext _context;
        
        /// <summary>
        /// The telemetry
        /// </summary>
        private TelemetryClient _telemetry;
        
        /// <summary>
        /// The telemetry configuration
        /// </summary>
        private TelemetryConfiguration _telemetryConfiguration;
        
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly AppInsightsTelemetryWriterConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsTelemetryWriter"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="context">The context.</param>
        public AppInsightsTelemetryWriter(AppInsightsTelemetryWriterConfiguration configuration, ITelemetryContext context)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
            SetField.NotNull(out _context, nameof(context), context);

            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
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

        /// <summary>
        /// Does the post log actions.
        /// </summary>
        private void DoPostLogActions()
        {
            if (_configuration.FlushEveryWrite) _telemetry.Flush();
        }

        /// <summary>
        /// write intent as an asynchronous operation.
        /// </summary>
        /// <param name="intentTelemetryData">The intent telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteIntentAsync(IIntentTelemetryData intentTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Intents))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("json", intentTelemetryData.Json);
                    properties.Add("name", intentTelemetryData.IntentName);
                    properties.Add("text", intentTelemetryData.IntentText);
                    properties.Add("hasAmbiguousEntities", intentTelemetryData.IntentHasAmbiguousEntities.ToString());

                    var metrics = new Dictionary<string, double>
                    {
                        {"score", intentTelemetryData.IntentConfidenceScore ?? 0d }
                    };

                    _telemetry.TrackEvent("Intent", properties, metrics);
                    DoPostLogActions();

                });
            }
        }

        /// <summary>
        /// write entity as an asynchronous operation.
        /// </summary>
        /// <param name="entityTelemetryData">The entity telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteEntityAsync(IEntityTelemetryData entityTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Entities))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("json", entityTelemetryData.Json);
                    properties.Add("type", entityTelemetryData.EntityType);
                    properties.Add("value", entityTelemetryData.EntityValue);
                    properties.Add("isAmbiguous", entityTelemetryData.EntityIsAmbiguous.ToString());

                    var metrics = new Dictionary<string, double>
                    {
                        {"score", entityTelemetryData.EntityConfidenceScore ?? 0d }
                    };

                    _telemetry.TrackEvent("Entity", properties, metrics);
                    DoPostLogActions();

                });
            }
        }

        /// <summary>
        /// write request as an asynchronous operation.
        /// </summary>
        /// <param name="requestTelemetryData">The request telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteRequestAsync(IRequestTelemetryData requestTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                var duration = requestTelemetryData.RequestStartDateTime.Subtract(requestTelemetryData.RequestEndDateTime).TotalMilliseconds;

                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("json", requestTelemetryData.Json);
                    properties.Add("millisecondsDuration", $"{duration}");
                    properties.Add("cacheHit", $"{requestTelemetryData.RequestIsCacheHit}");
                    properties.Add("isAmbiguous", $"{requestTelemetryData.RequestIsAmbiguous}");
                    properties.Add("quality", $"{requestTelemetryData.RequestQuality}");
                    properties.Add("isAuthenticated", $"{requestTelemetryData.RequestIsAuthenticated}");

                    _telemetry.TrackEvent("Request", properties);
                    DoPostLogActions();

                });
            }
        }

        /// <summary>
        /// write response as an asynchronous operation.
        /// </summary>
        /// <param name="responseTelemetryData">The response telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteResponseAsync(IResponseTelemetryData responseTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Responses))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("json", responseTelemetryData.Json);
                    properties.Add("text", responseTelemetryData.ResponseText);
                    properties.Add("imageUrl", responseTelemetryData.ResponseImageUrl);
                    properties.Add("responseJson", responseTelemetryData.ResponseJson);
                    properties.Add("type", responseTelemetryData.ResponseType);

                    _telemetry.TrackEvent("Response", properties);
                    DoPostLogActions();

                });
            }
        }

        /// <summary>
        /// write counter as an asynchronous operation.
        /// </summary>
        /// <param name="counterTelemetryData">The counter telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteCounterAsync(ICounterTelemetryData counterTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Counters))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();
                    properties.Add("json", counterTelemetryData.Json);
                    properties.Add("category", counterTelemetryData.CounterCategory);
                    properties.Add("name", counterTelemetryData.CounterName);

                    var metrics = new Dictionary<string, double> { { "count", counterTelemetryData.CounterValue } };

                    _telemetry.TrackEvent("Counter", properties, metrics);
                    DoPostLogActions();
                });
            }
        }

        /// <summary>
        /// write measure as an asynchronous operation.
        /// </summary>
        /// <param name="measureTelemetryData">The measure telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteMeasureAsync(IMeasureTelemetryData measureTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Measures))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();
                    properties.Add("json", measureTelemetryData.Json);
                    properties.Add("category", measureTelemetryData.MeasureCategory);
                    properties.Add("name", measureTelemetryData.MeasureName);

                    var metrics = new Dictionary<string, double> { { "measure", measureTelemetryData.MeasureValue } };

                    _telemetry.TrackEvent("Measure", properties, metrics);
                    DoPostLogActions();
                });
            }
        }

        /// <summary>
        /// write service result as an asynchronous operation.
        /// </summary>
        /// <param name="serviceResultTelemetryData">The service result telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.ServiceResults))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("json", serviceResultTelemetryData.Json);
                    properties.Add("name", serviceResultTelemetryData.ServiceResultName);
                    properties.Add("success", serviceResultTelemetryData.ServiceResultSuccess.ToString());
                    properties.Add("response", serviceResultTelemetryData.ServiceResultResponse);

                    var metrics = new Dictionary<string, double>
                    {
                        {"milliseconds", serviceResultTelemetryData.ServiceResultMilliseconds }
                    };

                    _telemetry.TrackEvent("ServiceResult", properties, metrics);
                    DoPostLogActions();
                });
            }

        }

        /// <summary>
        /// write exception as an asynchronous operation.
        /// </summary>
        /// <param name="exceptionTelemetryData">The exception telemetry data.</param>
        /// <returns>Task.</returns>
        public async Task WriteExceptionAsync(IExceptionTelemetryData exceptionTelemetryData)
        {
            if (_configuration.Handles(TelemetryTypes.Exceptions))
            {
                await Task.Run(() =>
                {
                    var properties = GetBotContextProperties();

                    properties.Add("json", exceptionTelemetryData.Json);
                    properties.Add("component", exceptionTelemetryData.ExceptionComponent);
                    properties.Add("context", exceptionTelemetryData.ExceptionContext);

                    _telemetry.TrackException(exceptionTelemetryData.Ex, properties);
                    DoPostLogActions();
                });
            }
        }

        /// <summary>
        /// Sets the telemetry context.
        /// </summary>
        /// <param name="context">The telemetry context.</param>
        public void SetContext(ITelemetryContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the bot context properties.
        /// </summary>
        /// <returns>Dictionary&lt;System.String, System.String&gt;.</returns>
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
        /// <summary>
        /// write event as an asynchronous operation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Task.</returns>
        public async Task WriteEventAsync(string key, string value)
        {
            await WriteEventAsync(new Dictionary<string, string> { { key, value } });
        }

        /// <summary>
        /// write event as an asynchronous operation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Task.</returns>
        public async Task WriteEventAsync(string key, double value)
        {
            await WriteEventAsync(new Dictionary<string, double> { { key, value } });
        }

        /// <summary>
        /// write event as an asynchronous operation.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <returns>Task.</returns>
        public async Task WriteEventAsync(Dictionary<string, double> metrics)
        {
            await WriteEventAsync(GetBotContextProperties(), metrics);
        }

        /// <summary>
        /// write event as an asynchronous operation.
        /// </summary>
        /// <param name="eventProperties">The event properties.</param>
        /// <param name="eventMetrics">The event metrics.</param>
        /// <returns>Task.</returns>
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