using System;

namespace Microsoft.Bot.Builder.Telemetry.Data
{
    public interface IExceptionTelemetry : ICommonTelemetry
    {
        string ExceptionComponent { get; set; }
        string ExceptionContext { get; set; }
        Exception Ex { get; set; }
    }
}