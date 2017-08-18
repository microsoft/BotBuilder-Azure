using System.Configuration;
using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Extensions.Azure.Telemetry.AppInsightsWriter
{
    /// <summary>
    /// Class AppInsightsTelemetryWriterConfiguration.
    /// </summary>
    /// <seealso cref="Microsoft.Bot.Builder.Extensions.Telemetry.TypeDiscriminatingTelemetryWriterConfigurationBase" />
    public class AppInsightsTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        /// <summary>
        /// The instrumentation key
        /// </summary>
        private string _instrumentationKey;
       
        /// <summary>
        /// Gets or sets a value indicating whether [flush every write].
        /// </summary>
        /// <value><c>true</c> if [flush every write]; otherwise, <c>false</c>.</value>
        public bool FlushEveryWrite { get; set; }

        /// <summary>
        /// Gets or sets the instrumentation key.
        /// </summary>
        /// <value>The instrumentation key.</value>
        /// <exception cref="ConfigurationErrorsException">InstrumentationKey</exception>
        public string InstrumentationKey
        {
            get
            {
                if (string.IsNullOrEmpty(_instrumentationKey))
                {
                    throw new ConfigurationErrorsException($"The {nameof(InstrumentationKey)} property was not set.  See https://docs.microsoft.com/en-us/azure/application-insights/app-insights-create-new-resource for more information.");
                }

                return _instrumentationKey;
            }
            set { _instrumentationKey = value; }
        }
    }
}