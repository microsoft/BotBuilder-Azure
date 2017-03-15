using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Log activities to Azure Storage Queue. 
    /// </summary>
    /// <remarks>
    /// Activities are limited to size of Azure Storage queue payload.  Disposition of larger messages is controlled by QueueLoggerSettings.
    /// for more information on limits see https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-azure-and-service-bus-queues-compared-contrasted
    /// </remarks>
    public class AzureQueueActivityLogger : IActivityLogger
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly CloudQueue _cloudQueue;
        private readonly QueueLoggerSettings _queueLoggerSettings;

        private readonly float _cutCoefficient;

        /// <summary>
        /// Constructs an instance of AzureQueueActivityLogger
        /// </summary>
        /// <param name="cloudQueue">Reference to a CloudQueue instance</param>
        /// <param name="queueSettings">Settings informing the logger how to handle large messages and whether compression is required</param>
        /// <param name="settings">JSON serialziation settings used to write the formatted JSON message before adding to the queue</param>
        public AzureQueueActivityLogger(CloudQueue cloudQueue, QueueLoggerSettings queueSettings = null, JsonSerializerSettings settings = null)
        {
            SetField.NotNull(out _cloudQueue, nameof(cloudQueue), cloudQueue);

            //set the defaults
            _queueLoggerSettings = queueSettings ?? new QueueLoggerSettings();
            _jsonSerializerSettings = settings;
            _cutCoefficient = 1 - _queueLoggerSettings.MessageTrimRate;
        }

        /// <summary>
        /// Logs a single Activity message
        /// </summary>
        /// <param name="activity">An activity to be logged</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">thrown when message exceeds length limit</exception>
        public async Task LogAsync(IActivity activity)
        {
            var message = activity.AsMessageActivity();

            var jsonMsg = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
            var bytes = GetBytes(jsonMsg);

            if (_queueLoggerSettings.OverflowHanding == LargeMessageMode.Discard)
            {
                //if fails, do not do anything....
                try
                {
                    await _cloudQueue.AddMessageAsync(new CloudQueueMessage(bytes));
                }
                catch
                {
                    // ignored
                }
            }
            else if (_queueLoggerSettings.OverflowHanding == LargeMessageMode.Error)
            {
                //let it fail                    
                await _cloudQueue.AddMessageAsync(new CloudQueueMessage(bytes));
            }
            else if (_queueLoggerSettings.OverflowHanding == LargeMessageMode.Trim)
            {
                do
                {
                    try
                    {
                        await _cloudQueue.AddMessageAsync(new CloudQueueMessage(bytes));
                        return;
                    }
                    catch (Exception)
                    {
                        //cut off some of the text to fit
                        message.Text = message.Text.Substring(0, (int)(message.Text.Length * _cutCoefficient));
                        jsonMsg = JsonConvert.SerializeObject(message, _jsonSerializerSettings);
                        bytes = GetBytes(jsonMsg);
                    }
                } while (true);
            }
        }

        byte[] GetBytes(string message)
        {
            return _queueLoggerSettings.CompressMessage ? message.Compress() : Encoding.UTF8.GetBytes(message);
        }
    }
}