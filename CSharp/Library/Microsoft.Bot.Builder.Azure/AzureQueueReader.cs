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
    public class AzureQueueReader: IQueueReader
    {
        private readonly CloudQueue _cloudQueue;
        private QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudQueue"></param>
        public AzureQueueReader(CloudQueue cloudQueue, QueueLoggerSettings queueSettings = null)
        {
            cloudQueue = cloudQueue ?? throw new ArgumentNullException("client is required");

            _queueLoggerSettings = queueSettings;

            //set the defaults
            if (_queueLoggerSettings == null)
                _queueLoggerSettings = new QueueLoggerSettings();

            _cloudQueue = cloudQueue;
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
            var msg = await _cloudQueue.GetMessageAsync();

            if (msg == null)
                return null;

            var data = DeserializeItem(msg);

            return data;
        }

        /// <summary>
        /// cloudQueue may contain more than one message, read the {messageCount} of messages from the cloudQueue
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

            var messageBatch = await _cloudQueue.GetMessagesAsync(messageCount);

            foreach (var msg in messageBatch)
            {
                batch.Add(DeserializeItem(msg));
            }

            return batch;
        }

    }
}
