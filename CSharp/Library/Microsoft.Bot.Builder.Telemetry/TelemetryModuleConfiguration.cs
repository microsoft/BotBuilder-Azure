using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class TelemetryModuleConfiguration
    {
        public IList<object> TelemetryConfigurations { get; set; }
        public IList<Type> TelemetryWriterTypes { get; set; }
        public IList<ITelemetryWriter> TelemetryWriterInstances { get; set; }
        public IList<Assembly> TelemetryWriterAssemblies { get; set; }


        public TelemetryModuleConfiguration()
        {
            TelemetryConfigurations = new List<object>();
            TelemetryWriterTypes = new List<Type>();
            TelemetryWriterInstances = new List<ITelemetryWriter>();
            TelemetryWriterAssemblies = new List<Assembly>();
        }
    }
}