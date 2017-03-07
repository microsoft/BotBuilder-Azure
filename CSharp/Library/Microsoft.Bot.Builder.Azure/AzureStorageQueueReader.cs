using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chronic;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// 
    /// </summary>
    public class AzureStorageQueueReader: IQueueReader
    {
        private readonly CloudQueue _queueClient;
        private QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        public AzureStorageQueueReader(CloudQueue client, QueueLoggerSettings queueSettings)
        {
            _queueLoggerSettings = queueSettings;
            _queueClient = client;
        }
        private Activity DeserializeItem(CloudQueueMessage msg)
        {
            string jsonActivity = "";

            //message is compressed
            if (_queueLoggerSettings.CompressMessage)
                jsonActivity = msg.AsBytes.Decompress();
            else
                jsonActivity = msg.AsString;

            var data = JsonConvert.DeserializeObject<Activity>(jsonActivity);
            return data;
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
            var msg = await _queueClient.GetMessageAsync();

            if (msg == null)
                return null;

            var data = DeserializeItem(msg);

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

            var messageBatch = await _queueClient.GetMessagesAsync(messageCount);

            foreach (var msg in messageBatch)
            {
                batch.Add(DeserializeItem(msg));
            }

            return batch;
        }

    }
}
