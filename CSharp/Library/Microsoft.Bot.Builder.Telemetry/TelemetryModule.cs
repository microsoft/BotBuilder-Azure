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
            RegisterDateTimeProvider(builder);

            RegisterTelemetryWriterConfigurations(builder);
            RegisterTelemetryWriterTypes(builder);
            RegisterTelemetryWriterInstances(builder);

            base.Load(builder);
        }

        private void RegisterDateTimeProvider(ContainerBuilder builder)
        {
            builder.RegisterType<DateTimeProvider>().SingleInstance();
        }

        private void RegisterTelemetryWriterInstances(ContainerBuilder builder)
        {
            foreach (var instance in _configuration.TelemetryWriterInstances)
            {
                builder.RegisterInstance(instance).As<ITelemetryWriter>().SingleInstance();
            }
        }

        private void RegisterTelemetryWriterTypes(ContainerBuilder builder)
        {
            foreach (var type in _configuration.TelemetryWriterTypes)
            {
                builder.RegisterType(type).AsImplementedInterfaces().SingleInstance();
            }
        }

        private void RegisterTelemetryWriterConfigurations(ContainerBuilder builder)
        {
            foreach (var configuration in _configuration.TelemetryWriterConfigurations)
            {
                builder.RegisterInstance(configuration).AsSelf().SingleInstance();
            }
        }
    }

    public class TelemetryModuleConfiguration
    {
        public TelemetryWriterDiscoveryStrategy WriterDiscoveryStrategy { get; set; }
        public IList<object> TelemetryWriterConfigurations { get; set; }
        public IList<Type> TelemetryWriterTypes { get; set; }
        public IList<ITelemetryWriter> TelemetryWriterInstances { get; set; }


        public TelemetryModuleConfiguration()
        {
            TelemetryWriterConfigurations = new List<object>();
            TelemetryWriterTypes = new List<Type>();
            TelemetryWriterInstances = new List<ITelemetryWriter>();
        }
    }

    [Flags]
    public enum TelemetryWriterDiscoveryStrategy
    {
        None = 0,
        ScanExplicitFileSystemLocation = 1,
        ScanAssemblyFileSystemLocation = 2,
        UseExplicitlyDeclaredTypes = 4,
        UseExplicitlyDeclaredInstances = 8,
    }
}