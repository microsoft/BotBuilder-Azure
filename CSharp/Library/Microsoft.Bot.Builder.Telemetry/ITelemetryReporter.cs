﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Telemetry.Data;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Telemetry
{
    public interface ITelemetryReporter : ISetTelemetryContext
    {
        Task ReportIntentAsync(IIntentTelemetryData intentTelemetryData);
        Task ReportResponseAsync(IResponseTelemetryData responseTelemetryData);
        Task ReportDialogImpressionAsync(string dialog);
        Task ReportServiceResultAsync(IServiceResultTelemetryData serviceResultTelemetryData);
        Task ReportEventAsync(string key, string value);
        Task ReportEventAsync(string key, double value);
        Task ReportEventAsync(Dictionary<string, double> metrics);
        Task ReportEventAsync(Dictionary<string, string> properties, Dictionary<string, double> metrics = null);
        Task SetContextFrom(IActivity activity, ITelemetryContext context = null);
    }
}
