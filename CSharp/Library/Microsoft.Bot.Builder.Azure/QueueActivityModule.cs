using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Chronic;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{


    /// <summary>
    /// 
    /// </summary>
    public class QueueActivityModule : Module
    {
        private string _connectionString;
        private string _queueName;
        private JsonSerializerSettings _settings;
        private CloudStorageAccount _cloudStorageAccount;
        private string _serviceBusConnectionString = null;
        private QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// Create a QueueLogger for a particular storage account and table name.
        /// </summary>
        /// <param name="connectionString">Azure ServiceBus connection string.</param>
        /// <param name="queueName">Where to log activities.</param>
        /// <param name="settings"></param>
        public QueueActivityModule(CloudStorageAccount account, string queueName, QueueLoggerSettings loggerSettings= null, JsonSerializerSettings settings = null)
        {
            _cloudStorageAccount = account ?? throw new ArgumentNullException("account must be provided");

            if (queueName == null || queueName == "")
                throw new ArgumentException("queue name must be provided");
            else
                _queueName = queueName;

            _queueLoggerSettings = loggerSettings ?? new QueueLoggerSettings();
            
            _settings = settings;
        }

        /// <summary>
        /// Create a QueueLogger for a particular storage account and table name.
        /// </summary>
        /// <param name="connectionString">Azure ServiceBus connection string.</param>
        /// <param name="queueName">Where to log activities.</param>
        /// <param name="settings"></param>
        public QueueActivityModule(string serviceBusConnectionString, string queueName, QueueLoggerSettings loggerSettings = null, JsonSerializerSettings settings = null)
        {
            if (serviceBusConnectionString == null || serviceBusConnectionString == "")
                throw new ArgumentException("serviceBusConnectionString must be provided");

            if (queueName == null || queueName == "")
                throw new ArgumentException("queueName must be provided");


            _serviceBusConnectionString = serviceBusConnectionString;
            _queueLoggerSettings = loggerSettings == null ? new QueueLoggerSettings() : loggerSettings;
            _queueName = queueName;
            _settings = settings;
        }

        /// <summary>
        /// Update builder with registration for TableLogger.
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
