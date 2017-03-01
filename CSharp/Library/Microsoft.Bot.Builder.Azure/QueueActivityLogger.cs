using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Chronic;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{

    /// <summary>
    /// 
    /// </summary>
    public class QueueActivityLogger : IActivityLogger
    {
        private readonly QueueClient _client;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="settings"></param>
        public QueueActivityLogger(QueueClient client, JsonSerializerSettings settings)
        {
            _jsonSerializerSettings = settings;
            _client = client;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public async Task LogAsync(IActivity activity)
        {
            var jsonMsg = JsonConvert.SerializeObject(activity, _jsonSerializerSettings);

            var message = new BrokeredMessage(jsonMsg);

            await _client.SendAsync(message);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class QueueActivityReader
    {
        private readonly QueueClient _queueClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public QueueActivityReader(QueueClient client)
        {
            _queueClient = client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IActivity Read()
        {
            return ReadAsync().GetAwaiter().GetResult();
        }

        public async Task<IActivity> ReadAsync()
        {
            var msg = await _queueClient.ReceiveAsync();

            if (msg == null)
                return null;

            var jsonMsg = msg.GetBody<string>();

            var data = JsonConvert.DeserializeObject<IActivity>(jsonMsg);

            //mark as processed
            msg.Complete();

            return data;
        }

        /// <summary>
        /// queue may contain more than one message, read the {messageCount} of messages from the queue
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <returns></returns>
        public List<IActivity> ReadBatch(int messageCount)
        {
            return ReadBatchAsync(messageCount).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <returns></returns>
        public async Task<List<IActivity>> ReadBatchAsync(int messageCount)
        {
            List<IActivity> batch = new List<IActivity>();

            var messageBatch = await _queueClient.ReceiveBatchAsync(messageCount);

            foreach (var msg in messageBatch)
            {
                var jsonMsg = msg.GetBody<string>();
                var data = JsonConvert.DeserializeObject<IActivity>(jsonMsg);
                batch.Add(data);
            }


            //mark batch as complete only once if we are returning them from the call
            messageBatch.ForEach(m => m.Complete());

            return batch;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class QueueActivityModule : Module
    {
        private string _connectionString;
        private string _queueName;
        private JsonSerializerSettings _settings;
        /// <summary>
        /// Create a QueueLogger for a particular storage account and table name.
        /// </summary>
        /// <param name="connectionString">Azure ServiceBus connection string.</param>
        /// <param name="queueName">Where to log activities.</param>
        /// <param name="settings"></param>
        public QueueActivityModule(string connectionString, string queueName, JsonSerializerSettings settings)
        {
            _connectionString = connectionString;
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

            NamespaceManager namespaceManager = NamespaceManager.CreateFromConnectionString(_connectionString);
            if (!namespaceManager.QueueExists(_queueName))
            {
                namespaceManager.CreateQueue(_queueName); ;
            }

            builder.RegisterInstance(_settings).AsSelf().SingleInstance();

            builder.RegisterInstance(QueueClient.CreateFromConnectionString(_connectionString, _queueName))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<QueueActivityModule>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }

}
