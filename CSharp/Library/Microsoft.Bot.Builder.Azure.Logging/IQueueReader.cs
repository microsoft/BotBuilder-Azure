using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

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
        /// Reads a batch of messages from the queue not to exceed the message count.
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        Task<List<Activity>> ReadBatchAsync(int messageCount, TimeSpan serviceWaitTime);
        /// <summary>
        /// Reads a batch of messages from the queue not to exceed the message count, asynchronously
        /// </summary>
        /// <param name="messageCount">Maximum number of messages to return</param>
        Task<List<Activity>> ReadBatchAsync(int messageCount);
    }
}