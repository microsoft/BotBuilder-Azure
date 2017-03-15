using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Reads messages from an Azure Storage Queue
    /// </summary>
    public class AzureQueueReader : IQueueReader
    {
        private readonly CloudQueue _cloudQueue;
        private readonly QueueLoggerSettings _queueLoggerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cloudQueue">Reference to a CloudQueue instance</param>
        /// <param name="queueSettings">Settings informing the logger how to handle large messages and whether compression is required</param>
        public AzureQueueReader(CloudQueue cloudQueue, QueueLoggerSettings queueSettings = null)
        {
            SetField.NotNull(out _cloudQueue, nameof(cloudQueue), cloudQueue);

            //set the defaults
            _queueLoggerSettings = queueSettings ?? new QueueLoggerSettings();
        }
        private Activity DeserializeItem(CloudQueueMessage msg)
        {
            string jsonActivity;

            //message is compressed
            if (_queueLoggerSettings.CompressMessage)
                jsonActivity = msg.AsBytes.Decompress();
            else
                jsonActivity = msg.AsString;

            var data = JsonConvert.DeserializeObject<Activity>(jsonActivity);

            return data;
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
            var msg = await _cloudQueue.GetMessageAsync();

            if (msg == null)
                return null;

            var data = DeserializeItem(msg);

            return data;
        }

        /// <summary>
        /// Queue may contain more than one message, read the {messageCount} of messages from the queue
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
        /// <param name="serviceWaitTime">This parameter is ignored</param>
        /// <returns></returns>
        public List<Activity> ReadBatch(int messageCount, TimeSpan serviceWaitTime)
        {
            return ReadBatch(messageCount);
        }

        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count.
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
        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count.
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        /// <param name="serviceWaitTime">This parameter is ignored</param>
        /// <returns></returns>
        public Task<List<Activity>> ReadBatchAsync(int messageCount, TimeSpan serviceWaitTime)
        {
            return ReadBatchAsync(messageCount);
        }
    }
}
