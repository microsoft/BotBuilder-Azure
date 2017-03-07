using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronic;
using Microsoft.Bot.Connector;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Interface for queue reader
    /// </summary>
    public interface IQueueReader
    {
        /// <summary>
        /// Read a single activity message from the queue
        /// </summary>
        IActivity Read();
        /// <summary>
        /// Read a single activity message from the queue, asynchronously
        /// </summary>
        Task<Activity> ReadAsync();
        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count.
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        List<Activity> ReadBatch(int messageCount);
        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count, asynchronously
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        Task<List<Activity>> ReadBatchAsync(int messageCount);

    }
    /// <summary>
    /// Reads messages from an Azure Service Bus Queue
    /// </summary>
    public class ServiceBusQueueReader : IQueueReader
    {
        private readonly QueueClient _queueClient;
        private readonly QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// Creates an instance of queue reader
        /// </summary>
        /// <param name="client">Reference to a CloudClient instance</param>
        /// <param name="queueSettings">Settings informing the logger how to handle large messages and whether compression is required</param>
        public ServiceBusQueueReader(QueueClient client, QueueLoggerSettings queueSettings = null)
        {
            client = client ?? throw new ArgumentNullException(nameof(client));

            //set the defaults
            _queueLoggerSettings = queueSettings ?? new QueueLoggerSettings();

            _queueClient = client;
        }

        /// <summary>
        /// Read a single activity message from the queue
        /// </summary>
        /// <returns></returns>
        public IActivity Read()
        {
            return ReadAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Read a single activity message from the queue, asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<Activity> ReadAsync()
        {
            var msg = await _queueClient.ReceiveAsync();

            if (msg == null)
                return null;

            var data = DeserializeItem(msg);

            //mark as processed
            msg.Complete();

            return data;
        }

        private Activity DeserializeItem(BrokeredMessage msg)
        {
            string jsonActivity;
            
            //message is compressed
            if (_queueLoggerSettings.CompressMessage)
                jsonActivity = msg.GetBody<byte[]>().Decompress();
            else
                jsonActivity = msg.GetBody<string>();

            var data = JsonConvert.DeserializeObject<Activity>(jsonActivity);
            return data;
        }

        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count.
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <returns></returns>
        public List<Activity> ReadBatch(int messageCount)
        {
            return ReadBatchAsync(messageCount).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count.
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <returns></returns>
        public async Task<List<Activity>> ReadBatchAsync(int messageCount)
        {
            List<Activity> batch = new List<Activity>();

            var messageBatch = await _queueClient.ReceiveBatchAsync(messageCount);

            //avoid multiple enumeration
            var brokeredMessages = messageBatch as IList<BrokeredMessage> ?? messageBatch.ToList();

            foreach (var msg in brokeredMessages)
            {
                batch.Add(DeserializeItem(msg));
            }

            //mark batch as complete only once if we are returning them from the call
            brokeredMessages.ForEach(m => m.Complete());

            return batch;
        }

    }
}
