using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Telemetry.Formatters;

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
            RegisterAllRequiredDefaultTypes(builder);
            RegisterAllTelemetryWriters(builder);

            RegisterTelemetryReporter(builder);

            base.Load(builder);
        }

        private void RegisterAllTelemetryWriters(ContainerBuilder builder)
        {
            RegisterTelemetryWriterConfigurations(builder);
            RegisterTelemetryWriterTypes(builder);
            RegisterTelemetryWriterInstances(builder);
        }

        private void RegisterAllRequiredDefaultTypes(ContainerBuilder builder)
        {
            RegisterDefaultDateTimeProvider(builder);
            RegisterDefaultTelemetryContext(builder);
            RegisterDefaultOutputFormatter(builder);
        }

        private void RegisterDefaultOutputFormatter(ContainerBuilder builder)
        {
            builder.RegisterType<MachineOptimizedOutputFormatter>().As<ITelemetryOutputFormatter>();
        }

        private void RegisterTelemetryReporter(ContainerBuilder builder)
        {
            builder.RegisterType<TelemetryReporter>().AsImplementedInterfaces().SingleInstance();
        }

        private void RegisterDefaultTelemetryContext(ContainerBuilder builder)
        {
            builder.RegisterType<TelemetryContext>().AsImplementedInterfaces();
        }

        private void RegisterDefaultDateTimeProvider(ContainerBuilder builder)
        {
            builder.RegisterType<DateTimeProvider>().AsImplementedInterfaces().SingleInstance();
        }

        private void RegisterTelemetryWriterInstances(ContainerBuilder builder)
        {
            foreach (var instance in _configuration.TelemetryWriterInstances)
            {
                builder.RegisterInstance(instance).AsImplementedInterfaces().SingleInstance();
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