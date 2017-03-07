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
    public interface IQueueReader
    {
        IActivity Read();
        List<Activity> ReadBatch(int messageCount);
        Task<List<Activity>> ReadBatchAsync(int messageCount);
        Task<Activity> ReadAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    public class ServiceBusQueueReader : IQueueReader
    {
        private readonly QueueClient _queueClient;
        private QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public ServiceBusQueueReader(QueueClient client, QueueLoggerSettings loggerSettings = null)
        {
            client = client ?? throw new ArgumentNullException("client is required");

            _queueLoggerSettings = loggerSettings;
            //set the defaults
            if (_queueLoggerSettings == null)
                _queueLoggerSettings = new QueueLoggerSettings();

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

        /// <summary>
        /// 
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
            string jsonActivity = "";
            
            //message is compressed
            if (_queueLoggerSettings.CompressMessage)
                jsonActivity = msg.GetBody<byte[]>().Decompress();
            else
                jsonActivity = msg.GetBody<string>();

            var data = JsonConvert.DeserializeObject<Activity>(jsonActivity);
            return data;
        }

        /// <summary>
        /// queue may contain more than one message, read the {messageCount} of messages from the queue
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <returns></returns>
        public List<Activity> ReadBatch(int messageCount)
        {
            return ReadBatchAsync(messageCount).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <returns></returns>
        public async Task<List<Activity>> ReadBatchAsync(int messageCount)
        {
            List<Activity> batch = new List<Activity>();

            var messageBatch = await _queueClient.ReceiveBatchAsync(messageCount);

            foreach (var msg in messageBatch)
            {
                batch.Add(DeserializeItem(msg));
            }

            //mark batch as complete only once if we are returning them from the call
            messageBatch.ForEach(m => m.Complete());

            return batch;
        }

    }
}
