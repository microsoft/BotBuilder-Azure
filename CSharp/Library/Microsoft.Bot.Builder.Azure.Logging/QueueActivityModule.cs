using System;
using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Module responsible for handling queue operations for Activities
    /// </summary>
    public class QueueActivityModule : Module
    {
        private readonly string _queueName;
        private readonly JsonSerializerSettings _settings;
        private readonly CloudStorageAccount _cloudStorageAccount;
        private readonly string _serviceBusConnectionString = null;
        private readonly QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// Registers IActivityLogger for Azure Storage Queue.
        /// </summary>
        /// <param name="account">Azure storage account</param>
        /// <param name="queueName">Azure storage queue</param>
        /// <param name="loggerSettings">Logger settings</param>
        /// <param name="settings">JSON serialization settings for message serialization before enqueing</param>
        public QueueActivityModule(CloudStorageAccount account, string queueName, QueueLoggerSettings loggerSettings = null, JsonSerializerSettings settings = null)
        {

            SetField.NotNull(out _cloudStorageAccount, nameof(account), account);

            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException("queue name must be provided");
            else
                _queueName = queueName;

            _queueLoggerSettings = loggerSettings ?? new QueueLoggerSettings();
            _settings = settings;
        }

        /// <summary>
        /// Registers IActivityLogger for Service Bus Queue.
        /// </summary>
        /// <param name="serviceBusConnectionString">Connection string to the Service Bus resource</param>
        /// <param name="queueName">Azure storage queue</param>
        /// <param name="loggerSettings">Logger settings</param>
        /// <param name="settings">JSON serialization settings for message serialization before enqueing</param>
        public QueueActivityModule(string serviceBusConnectionString, string queueName, QueueLoggerSettings loggerSettings = null, JsonSerializerSettings settings = null)
        {
            if (string.IsNullOrEmpty(serviceBusConnectionString))
                throw new ArgumentException("serviceBusConnectionString must be provided");

            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentException("queueName must be provided");

            _serviceBusConnectionString = serviceBusConnectionString;
            _queueLoggerSettings = loggerSettings ?? new QueueLoggerSettings();
            _queueName = queueName;
            _settings = settings;
        }

        /// <summary>
        /// Update builder with registration for IActivityLogger.
        /// </summary>
        /// <param name="builder">Builder to use for registration.</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            if (_serviceBusConnectionString == null)
            {
                RegisterStorageQueue(builder);
            }
            else
            {
                RegisterServiceBusQueue(builder);
            }

            builder.RegisterInstance(_queueLoggerSettings).AsSelf().SingleInstance();

            if (_settings != null)
                builder.RegisterInstance(_settings).AsSelf().SingleInstance();

            builder.RegisterType<QueueActivityModule>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private void RegisterStorageQueue(ContainerBuilder builder)
        {
            var queue = _cloudStorageAccount.CreateCloudQueueClient().GetQueueReference(_queueName);

            queue.CreateIfNotExists();

            builder.RegisterInstance(queue)
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<AzureQueueActivityLogger>().AsImplementedInterfaces();

        }

        private void RegisterServiceBusQueue(ContainerBuilder builder)
        {

            NamespaceManager namespaceManager =
                NamespaceManager.CreateFromConnectionString(_serviceBusConnectionString);

            namespaceManager.CreateQueue(_queueName);

            var queue = QueueClient.CreateFromConnectionString(_serviceBusConnectionString, _queueName);

            builder.RegisterInstance(queue)
                .As<QueueClient>()
                .SingleInstance();

            builder.RegisterType<ServiceBusActivityLogger>().AsImplementedInterfaces();
        }
    }
}
