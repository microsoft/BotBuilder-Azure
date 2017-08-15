using System.Configuration;
using Microsoft.Bot.Builder.Extensions.Telemetry;

namespace Microsoft.Bot.Builder.Azure.Extensions.Telemetry.AppInsightsWriter
{
    public class AppInsightsTelemetryWriterConfiguration : TypeDiscriminatingTelemetryWriterConfigurationBase
    {
        private string _instrumentationKey;
        public bool FlushEveryWrite { get; set; }

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