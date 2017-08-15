using System;

namespace Microsoft.Bot.Builder.Extensions.Telemetry.Data
{
    public interface IExceptionTelemetryData : ICommonTelemetryData
    {
        string ExceptionComponent { get; set; }
        string ExceptionContext { get; set; }
        Exception Ex { get; set; }
        Type ExceptionType { get; }
        string ExceptionMessage { get; }
        string ExceptionDetail { get; }
    }
}