using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Microsoft.Bot.Builder.Autofac.Base;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Telemetry
{
    public class TelemetryModule : Module
    {
        private readonly TelemetryModuleConfiguration _configuration;

        public TelemetryModule(TelemetryModuleConfiguration configuration)
        {
            SetField.NotNull(out _configuration, nameof(configuration), configuration);
        }

        protected override void Load(ContainerBuilder builder)
        {

            //ensure all provided Configuration instances are registered
            foreach (var configuration in _configuration.TelemetryWriterConfigurations)
            {
                builder.RegisterInstance(configuration).AsSelf().SingleInstance();
            }

            base.Load(builder);
        }

    }

    public class TelemetryModuleConfiguration
    {
        public TelemetryWriterDiscoveryStrategy WriterDiscoveryStrategy { get; set; }
        public IList<object> TelemetryWriterConfigurations { get; set; }


        public TelemetryModuleConfiguration()
        {
            TelemetryWriterConfigurations = new List<object>();
        }
    }

    [Flags]
    public enum TelemetryWriterDiscoveryStrategy
    {
        ExplicitFileSystemLocation = 1,
        AssemblyFileSystemLocation = 2,
        ExplicitlyDeclared = 4,
        PreregisteredWithContainer = 8,
    }
}